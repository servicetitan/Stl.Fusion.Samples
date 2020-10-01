import { useRef } from "react";
import deepEqual from "lodash/isEqual";

export function useSameObject(newObject) {
  const ref = useRef(null);

  if (!deepEqual(ref.current, newObject)) {
    ref.current = newObject;
  }

  return ref.current;
}

export function isDocumentVisible() {
  if (
    typeof document !== "undefined" &&
    typeof document.visibilityState !== "undefined"
  ) {
    return document.visibilityState !== "hidden";
  }
  // always assume it's visible
  return true;
}

export function isOnline() {
  if (typeof navigator.onLine !== "undefined") {
    return navigator.onLine;
  }
  // always assume it's online
  return true;
}
