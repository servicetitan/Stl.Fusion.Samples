import { useState, useEffect, useRef } from "react";
import { useSameObject } from "./utils";
import createFusionSubscription from "./Fusion";

export default function useFusionSubscription(url, argParams, argConfig) {
  const [result, setResult] = useState({
    loading: true,
    error: undefined,
    data: undefined,
  });

  const params = useSameObject(argParams);
  const config = useSameObject(argConfig);

  const unsubscribe = useRef(null);

  useEffect(() => {
    let isMounted = true;

    // Allow the user to pass null "url" arg to unsubscribe
    if (url) {
      unsubscribe.current = createFusionSubscription(
        url,
        params,
        config
      ).subscribeToUpdates((...args) => {
        if (isMounted) {
          setResult(...args);
        }
      });
    }

    return () => {
      isMounted = false;

      return unsubscribe.current?.();
    };
  }, [url, params, config]);

  // TODO: Also return cancel?
  return result;
}
