import React from "react";
import StlFusionContext from "./StlFusionContext";

export default function StlFusionConfig({ children, uri, ...options }) {
  return (
    <StlFusionContext.Provider value={{ uri, options }}>
      {children}
    </StlFusionContext.Provider>
  );
}
