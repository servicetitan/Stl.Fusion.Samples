import { useRef } from "react";
import deepEqual from "lodash/isEqual";

export function useSameObject(newObject) {
  const ref = useRef(null);

  if (!deepEqual(ref.current, newObject)) {
    ref.current = newObject;
  }

  return ref.current;
}
