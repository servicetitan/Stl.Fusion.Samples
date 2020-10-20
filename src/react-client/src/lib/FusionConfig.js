import React from "react";
import FusionContext from "./FusionContext";

export default function StlFusionConfig({ children, uri, ...options }) {
  return (
    <FusionContext.Provider value={{ uri, options }}>
      {children}
    </FusionContext.Provider>
  );
}
