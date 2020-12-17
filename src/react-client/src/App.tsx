import React, { useState } from "react";
import Wait from "./Wait";
import TimeMobX from "./TimeMobX";
import TimeHooks from "./TimeHooks";
import ComposerHooks from "./ComposerHooks";
import ComposerMobX from "./ComposerMobX";
import Chat from "./Chat";
import Sum from "./Sum";
import Auth from "./Auth";

function App() {
  const [wait, setWait] = useState(600);

  return (
    <div className="grid max-w-4xl grid-cols-1 gap-5 px-8 pt-8 mx-auto text-sm text-gray-900">
      <Wait wait={wait} onWaitChange={setWait} />
      <TimeMobX />
      <TimeHooks />
      <Auth />
      <ComposerMobX />
      <ComposerHooks />
      <Chat />
      <Sum />
    </div>
  );
}

export default App;
