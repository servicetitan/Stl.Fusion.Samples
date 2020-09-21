import { useState, useEffect, useRef, useContext } from "react";
import throttle from "lodash/throttle";
import { v4 as uuidv4 } from "uuid";
import StlFusionContext from "./StlFusionContext";
import DEFAULT_FETCHER from "./defaultFetcher";

// TODO: Write an actual README

// TODO: Add example with removing throttle delay after API call
// TODO: Add example with optimistic update after API call

// TODO: Reconnect on document.visibilityState (similar to SWR/react-query)

// TODO: User should have to config URI and default options... throw error if they don't?
// TODO: Allow user to fetch data however they want (fetch, axios, XHR),
//       probably by providing a promise-returning function
// TODO: ??? Allow user to set default headers, auth tokens, etc
//       (could they just do this in the configured fetcher function?)
// TODO: Allow user to configure clientId?
// TODO: Defaults could be set via a wrapper component,
//       then global state could live in context?
// TODO: Figure out what optimization needs to happen so that updating
//       context only re-renders the child components that care.

// TODO: Doesn't currently work if different components ask for
//       different throttle wait times. (First one wins.)

// {
//   clientId: "uuidv4",
//   publishers: Map
//     [PublisherId]: {
//       socket: new WebSocket(),
//       publications: {
//         [PublicationId]: Map
//           throttledRequestUpdate: f(),
//           setStates: Set,
//           cancelWait: false
//         }
//       }
//     }
//   }
// }

// config: {
//   uri: "...",
//   options: {
//     wait: 300,
//     fetcher: f()
//   }
// }

// constants
const MESSAGE_HEADER =
  "Stl.Fusion.Bridge.Messages.SubscribeMessage, Stl.Fusion";
const DEFAULT_URI = `${
  window.location.protocol === "https:" ? "wss:" : "ws:"
}//${window.location.host}/fusion/ws`;
const DEFAULT_WAIT = 300;

// global state
let STL = {
  clientId: uuidv4(),
  publishers: new Map(),
};

export default function useStlFusion(
  url,
  params,
  overrideConfig = { options: {} }
) {
  const [result, setResult] = useState({
    loading: true,
    error: undefined,
    data: undefined,
  });

  const contextConfig = useContext(StlFusionContext);

  const uri = overrideConfig.uri ?? contextConfig.uri ?? DEFAULT_URI;
  const wait =
    overrideConfig.options.wait ?? contextConfig.options?.wait ?? DEFAULT_WAIT;
  const fetcher =
    overrideConfig.options.fetcher ??
    contextConfig.options?.fetcher ??
    DEFAULT_FETCHER;

  const publicationRef = useRef(null);

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
      publication.setState.delete(setResult);

      // if empty, remove publication
      if (publication.setState.size === 0) {
        publisher.publications.delete(PublicationId);
      }

      // if empty, close socket and remove publisher
      if (publisher.publications.size === 0) {
        publisher.socket.close(1000);
        STL.publishers.delete(PublisherId);
      }
    };
  }, [url, params, uri, wait, fetcher]);

  return result;
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
    publication.throttledRequestUpdate(socket, data);

    // if cancelWait === false
    //    call throttled update request using socket
    // else
    //    updateRequest.cancel
    //    updateRequest (no throttle)
    //    cancelWait = false
  }

  if (data.Output) {
    // console.log("data.Output", data);

    publication.setState.forEach((setResult) => {
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
      setState: new Set([setResult]),
      throttledRequestUpdate: throttle(sendRequestUpdateMessage, wait, {
        leading: false,
      }),
      cancelWait: false,
    });

    sendSubscribeMessage(socket, { data });
  } else {
    publisher.publications.get(PublicationId).setState.add(setResult);
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
