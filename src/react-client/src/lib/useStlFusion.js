import { useState, useEffect, useRef, useContext, useCallback } from "react";
import merge from "lodash/merge";
import throttle from "lodash/throttle";
import { v4 as uuidv4 } from "uuid";
import StlFusionContext from "./StlFusionContext";
import DEFAULT_FETCHER from "./defaultFetcher";

// TODO: Write an actual README

// TODO: Add example with removing throttle delay after API call
// TODO: Add example with optimistic update after API call

// TODO: Reconnect on document.visibilityState (similar to SWR/react-query)

// TODO: Allow user to fetch data however they want (fetch, axios, XHR),
//       probably by providing a promise-returning function
// TODO: ??? Allow user to set default headers, auth tokens, etc
//       (could they just do this in the configured fetcher function?)
// TODO: ??? Allow user to configure clientId

// TODO: If different components ask for different throttle wait times for
//       the same API endpoint, the first one wins.

// TODO: Should multiple calls to the same endpoint try to use existing value
//       instead of each making their own API calls?

// {
//   clientId: "uuidv4",
//   publishers: Map
//     [PublisherId]: {
//       socket: new WebSocket(),
//       publications: {
//         [PublicationId]: Map
//           throttledRequestUpdate: f(),
//           setResults: Set,
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
};

export default function useStlFusion(url, params, overrideConfig) {
  const publicationRef = useRef(null);

  const [result, setResult] = useState({
    loading: true,
    error: undefined,
    data: undefined,
  });

  const cancel = useCallback(() => {
    if (publicationRef.current) {
      const { PublisherId, PublicationId } = publicationRef.current;
      const publisher = STL.publishers.get(PublisherId);
      const publication = publisher?.publications.get(PublicationId);

      if (publisher && publication) {
        publication.throttledRequestUpdate.cancel();
        sendRequestUpdateMessage(publisher.socket, publicationRef.current);
      }
    }
  }, []);

  const contextConfig = useContext(StlFusionContext);

  const {
    uri,
    options: { wait, fetcher },
  } = merge({}, DEFAULT_CONFIG, contextConfig, overrideConfig);

  useEffect(() => {
    let isMounted = true;

    // Allow the user to pass null "url" arg to unsubscribe
    if (url != null) {
      fetcher(url, params)
        .then(async ({ data, headers }) => {
          if (isMounted) {
            // update the state with REST call response
            setResult({ loading: false, data });

            // store the publication data
            publicationRef.current = headers;

            // wire up all the websocket stuff
            const config = { uri, options: { wait } };
            const socket = await createPublisher(headers, config);
            createPublication(socket, headers, config, setResult);
          }
        })
        .catch((error) => {
          if (isMounted) {
            setResult({ loading: false, error });
            // throw error; // makes promises easier to debug
          }
        });
    }

    return () => {
      isMounted = false;

      if (!publicationRef.current) {
        return;
      }

      const { PublisherId, PublicationId } = publicationRef.current;
      const publisher = STL.publishers.get(PublisherId);
      const publication = publisher.publications.get(PublicationId);

      // remove set state
      publication.setResults.delete(setResult);

      // if empty, remove publication
      if (publication.setResults.size === 0) {
        publisher.publications.delete(PublicationId);
      }

      // if empty, close socket and remove publisher
      if (publisher.publications.size === 0) {
        publisher.socket.close(1000);
        STL.publishers.delete(PublisherId);
      }
    };
  }, [url, params, uri, wait, fetcher]);

  return { ...result, cancel };
}

/**
 *
 */
function createPublisher({ PublisherId }, { uri }) {
  return new Promise((resolve) => {
    let socket;

    // for each new PublisherId, open a socket connection, save it, and subscribe to incoming messages
    if (!STL.publishers.has(PublisherId)) {
      socket = new WebSocket(
        `${uri}?publisherId=${PublisherId}&clientId=${STL.clientId}`
      );

      STL.publishers.set(PublisherId, { socket, publications: new Map() });

      socket.addEventListener("message", (event) => {
        const data = JSON.parse(event.data);
        handlePublicationMessage(socket, data);
      });

      socket.addEventListener("error", (event) => {
        console.error("WebSocket error observed:", event);
        // throw new Error(event);
      });
    } else {
      socket = STL.publishers.get(PublisherId).socket;
    }

    // if the socket connection isn't ready yet, wait until it's open before we resolve
    if (socket.readyState < 1) {
      socket.addEventListener("open", () => {
        resolve(socket);
      });
    } else {
      resolve(socket);
    }
  });
}

function handlePublicationMessage(socket, data) {
  const { PublisherId, PublicationId } = data;

  if (
    !STL.publishers.has(PublisherId) ||
    !STL.publishers.get(PublisherId).publications.has(PublicationId)
  ) {
    return;
  }

  const publication = STL.publishers
    .get(PublisherId)
    .publications.get(PublicationId);

  if (data.IsConsistent === false) {
    // to avoid slamming the server, we usually add a small delay to update requests
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

function createPublication(socket, data, { options: { wait } }, setResult) {
  const { PublisherId, PublicationId } = data;
  const publisher = STL.publishers.get(PublisherId);

  if (!publisher.publications.has(PublicationId)) {
    publisher.publications.set(PublicationId, {
      setResults: new Set([setResult]),
      throttledRequestUpdate: throttle(sendRequestUpdateMessage, wait, {
        leading: false,
      }),
    });

    sendSubscribeMessage(socket, { data });
  } else {
    publisher.publications.get(PublicationId).setResults.add(setResult);
  }
}

function sendSubscribeMessage(socket, { data }) {
  socket.send(
    `${MESSAGE_HEADER}|${JSON.stringify({
      ...data,
      $type: MESSAGE_HEADER,
      IsConsistent: true,
      IsUpdateRequested: false,
    })}`
  );
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
