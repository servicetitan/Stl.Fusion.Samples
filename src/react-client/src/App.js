import React, { useState } from "react";
import StlFusionConfig from "./lib/StlFusionConfig";
import Wait from "./Wait";
import Time from "./Time";
import Composer from "./Composer";
import Chat from "./Chat";

function App() {
  const [wait, setWait] = useState(300);

  return (
    <StlFusionConfig uri="ws://localhost:5005/fusion/ws" wait={wait}>
      <div className="grid max-w-4xl grid-cols-1 gap-5 px-8 pt-8 mx-auto text-sm text-gray-900">
        <Wait wait={wait} onWaitChange={setWait} />
        <Time />
        <Composer />
        <Chat />
      </div>
    </StlFusionConfig>
  );
}

export default App;
