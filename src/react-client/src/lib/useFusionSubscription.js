import { useState, useEffect, useContext, useCallback } from "react";
import merge from "lodash/merge";
import throttle from "lodash/throttle";
import { v4 as uuidv4 } from "uuid";
import FusionContext from "./FusionContext";
import DEFAULT_FETCHER from "./defaultFetcher";
import { useSameObject, isDocumentVisible, isOnline } from "./utils";

// TODO: Add concept of grouping multiple API calls together and update all
//       inconsistent data in the group when one member would update
// TODO: Decouple code from React to allow integration with Redux/MobX/Vue/etc
// TODO: Bugfix for different components configuring different throttle times for
//       the same API endpoint. (Currently, the first one wins.)
// TODO: Should multiple calls to the same endpoint try to use existing value
//       instead of each making their own API calls? (Can values be keyed by url?)

// {
//   clientId: "uuidv4",
//   waitingForReconnect: Set
//   publishers: Map
//     [PublisherId]: {
//       socket: new WebSocket(),
//       publications: {
//         [PublicationId]: Map
//           throttledRequestUpdate: f(),
//           setResults: Set,
//           resets: Set,
//         }
//       }
//     }
//   }
// }

// constants
const MESSAGE_HEADER =
  "Stl.Fusion.Bridge.Messages.SubscribeMessage, Stl.Fusion";
const DEFAULT_URI = `${
  window.location.protocol === "https:" ? "wss:" : "ws:"
}//${window.location.host}/fusion/ws`;
const DEFAULT_WAIT = 300;
const DEFAULT_CONFIG = {
  uri: DEFAULT_URI,
  options: {
    wait: DEFAULT_WAIT,
    fetcher: DEFAULT_FETCHER,
  },
};

// global state
let STL = {
  clientId: uuidv4(),
  publishers: new Map(),
  waitingForReconnect: new Set(),
};

// global state helpers
function getPublisher(PublicationRef) {
  const PublisherId = PublicationRef?.PublisherId;
  if (PublisherId && STL.publishers.has(PublisherId)) {
    return STL.publishers.get(PublisherId);
  }

  return null;
}

function getPublication(PublicationRef) {
  const publisher = getPublisher(PublicationRef);
  const PublicationId = PublicationRef?.PublicationId;

  if (publisher && publisher.publications.has(PublicationId)) {
    return publisher.publications.get(PublicationId);
  }

  return null;
}

// reconnecting when we're back online
window.addEventListener("visibilitychange", checkForReconnect, false);
window.addEventListener("focus", checkForReconnect, false);
window.addEventListener("online", checkForReconnect, false);

function checkForReconnect() {
  if (isDocumentVisible() && isOnline() && STL.waitingForReconnect.size > 0) {
    STL.waitingForReconnect.forEach((reconnect) => reconnect());
    STL.waitingForReconnect = new Set();
  }
}

/**
 * useFusionSubscription
 */
export default function useFusionSubscription(url, argParams, argConfig) {
  const [result, setResult] = useState({
    loading: true,
    error: undefined,
    data: undefined,
  });

  const [publicationIds, setPublicationIds] = useState(null);

  // used to re-render when the initial API call fails
  const [retryCount, setRetryCount] = useState(0);

  // reset function so we'll make the initial API call again if the WebSocket fails
  const [tick, setTick] = useState(0);
  const reset = useCallback(() => {
    setResult((result) => ({ ...result, loading: true }));
    setPublicationIds(null);
    setTick((tick) => tick + 1); // force update
  }, []);

  // cancel function to immediately request an update
  const cancel = useCallback(() => {
    if (publicationIds) {
      const publisher = getPublisher(publicationIds);
      const publication = getPublication(publicationIds);

      if (publisher && publication) {
        publication.throttledRequestUpdate.cancel();
        sendRequestUpdateMessage(publisher.socket, publicationIds);
      }
    }
  }, [publicationIds]);

  const params = useSameObject(argParams);
  const contextConfig = useContext(FusionContext);
  const config = useSameObject(
    merge({}, DEFAULT_CONFIG, contextConfig, argConfig)
  );

  useEffect(() => {
    let isMounted = true;
    let currentPublicationIds = publicationIds;

    // Allow the user to pass null "url" arg to unsubscribe
    if (url != null) {
      // if we're not online yet, wait until later
      if (!isDocumentVisible() || !isOnline()) {
        STL.waitingForReconnect.add(reset);
      } else {
        (async () => {
          // if we don't have PublisherId/PublicationId, make the initial API call
          if (!publicationIds) {
            try {
              const { data, header } = await config.options.fetcher(
                url,
                params
              );

              if (isMounted) {
                // update the state with REST call response
                setResult({ loading: false, data });

                // store the publication data and hold on to it locally for this run
                setPublicationIds(header.PublicationRef);
                currentPublicationIds = header.PublicationRef;

                // reset the retry counter since we succeeded
                setRetryCount(0);
              }
            } catch (error) {
              if (isMounted) {
                setResult({ loading: false, error });

                // if the initial API call fails, try again with exponential backoff
                await delay(1000 * 2 ** retryCount);
                setRetryCount((retryCount) => retryCount + 1);

                // throw error; // makes promises easier to debug
              }
            }
          }

          // create publisher if it doesn't exist
          let publisher = getPublisher(currentPublicationIds);
          if (currentPublicationIds && !publisher) {
            const { PublisherId } = currentPublicationIds;
            publisher = { publications: new Map() };
            STL.publishers.set(PublisherId, publisher);
          }

          // if the socket hasn't been created yet or if it's closed,
          // create a new socket
          if (
            publisher &&
            (!publisher.socket || publisher.socket.readyState > 1)
          ) {
            publisher.socket = createSocket(currentPublicationIds, config);
          }

          if (publisher && publisher.socket) {
            // wait for the socket to be ready
            try {
              const socket = await getOpenedSocket(publisher.socket);

              createPublication(
                socket,
                currentPublicationIds,
                config,
                setResult,
                reset
              );
            } catch (err) {
              // TODO: Handle case where socket is hanging and won't open
            }
          }
        })();
      }
    }

    return () => {
      isMounted = false;

      if (currentPublicationIds) {
        const { PublisherId, PublicationId } = currentPublicationIds;
        const publisher = getPublisher(currentPublicationIds);
        const publication = getPublication(currentPublicationIds);

        if (publisher && publication) {
          // remove set state functions
          publication.setResults.delete(setResult);
          publication.resets.delete(reset);

          // if empty, remove publication
          if (publication.setResults.size === 0) {
            publisher.publications.delete(PublicationId);
          }

          // if empty, close socket and remove publisher
          // HACK: add a delay so we don't unnecessarily churn through sockets
          if (publisher.publications.size === 0) {
            setTimeout(() => {
              if (publisher.publications.size === 0) {
                publisher.socket.close(1000);
                STL.publishers.delete(PublisherId);
              }
            }, 1000);
          }
        }
      }
    };
    // NOTE: "tick" needs to be in the dependencies array for "reset"
  }, [url, params, config, publicationIds, retryCount, tick, reset]);

  return { ...result, cancel };
}

function delay(ms) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}

function getOpenedSocket(socket) {
  return new Promise((resolve, reject) => {
    // if the socket connection isn't ready yet, wait until it's open
    if (socket.readyState < 1) {
      socket.addEventListener("open", () => {
        resolve(socket);
      });
    } else {
      resolve(socket);
    }

    setTimeout(reject, 30000); // 30-second timeout
  });
}

function createSocket({ PublisherId }, { uri }) {
  const socket = new WebSocket(
    `${uri}?publisherId=${PublisherId}&clientId=${STL.clientId}`
  );

  socket.addEventListener("message", (event) => {
    const data = JSON.parse(event.data);
    handlePublicationMessage(socket, data);
  });

  socket.addEventListener("error", (event) => {
    console.error("WebSocket error observed:", event);
    // throw new Error(event);
  });

  socket.addEventListener("close", (event) => {
    console.error("WebSocket closed:", event);
    // throw new Error(event);

    const publisher = getPublisher({ PublisherId });

    if (publisher) {
      publisher.publications.forEach((publication) => {
        publication.setResults.forEach((setResult) => {
          setResult((result) => ({ ...result, error: new Error(event) }));
        });

        if (isDocumentVisible() && isOnline()) {
          publication.resets.forEach((reset) => reset());
        } else {
          publication.resets.forEach((reset) => {
            STL.waitingForReconnect.add(reset);
          });
        }
      });
    }
  });

  return socket;
}

function handlePublicationMessage(socket, data) {
  const publication = getPublication(data);

  if (!publication) {
    return null;
  }

  if (data.IsConsistent === false) {
    // NOTE: We usually add a throttle delay to update requests
    publication.throttledRequestUpdate(socket, data);
  }

  if (data.Output) {
    publication.setResults.forEach((setResult) => {
      setResult({
        loading: false,
        data: data.Output.UnsafeValue,
      });
    });
  }
}

function createPublication(
  socket,
  data,
  { options: { wait } },
  setResult,
  reset
) {
  const { PublicationId } = data;
  const publisher = getPublisher(data);
  let publication = getPublication(data);

  if (!publication) {
    publication = {
      setResults: new Set(),
      resets: new Set(),
      throttledRequestUpdate: throttle(sendRequestUpdateMessage, wait, {
        leading: false,
      }),
    };

    publisher.publications.set(PublicationId, publication);

    sendSubscribeMessage(socket, data);
  }

  publication.setResults.add(setResult);
  publication.resets.add(reset);
}

function sendSubscribeMessage(socket, data) {
  if (socket.readyState === 1) {
    socket.send(
      `${MESSAGE_HEADER}|${JSON.stringify({
        ...data,
        $type: MESSAGE_HEADER,
        IsConsistent: true,
        IsUpdateRequested: false,
      })}`
    );
  }
}

function sendRequestUpdateMessage(
  socket,
  { PublisherId, PublicationId, Version }
) {
  // since this call is usually throttled, it's possible the socket
  // has closed by the time we attempt to send this message
  if (socket.readyState === 1) {
    socket.send(
      `${MESSAGE_HEADER}|${JSON.stringify({
        $type: MESSAGE_HEADER,
        PublisherId,
        PublicationId,
        Version,
        IsConsistent: false,
        IsUpdateRequested: true,
      })}`
    );
  }
}
