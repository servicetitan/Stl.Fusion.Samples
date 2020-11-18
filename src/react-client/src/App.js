import React, { useState } from "react";
import FusionConfig from "./lib/FusionConfig";
import Wait from "./Wait";
import MobX from "./MobX";
import Time from "./Time";
import Composer from "./Composer";
import Chat from "./Chat";
import Sum from "./Sum";
import Auth from "./Auth";

function App() {
  const [wait, setWait] = useState(600);

  return (
    <FusionConfig uri="ws://localhost:5005/fusion/ws" wait={wait}>
      <div className="grid max-w-4xl grid-cols-1 gap-5 px-8 pt-8 mx-auto text-sm text-gray-900">
        <Wait wait={wait} onWaitChange={setWait} />
        <MobX />
        <Time />
        <Sum />
        <Composer />
        <Chat />
        <Auth />
      </div>
    </FusionConfig>
  );
}

export default App;
