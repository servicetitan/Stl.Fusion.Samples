import React, { useState } from "react";
import formatDate from "date-fns/format";
import { makeAutoObservable } from "mobx";
import { observer } from "mobx-react-lite";
import { fusion, makeFusionObservable } from "stl.fusion";
import Section from "./Section";

class TimeStoreWithDecorators {
  @fusion("/api/Time/get")
  time = "";

  constructor() {
    makeAutoObservable(this);
    makeFusionObservable(this);
  }
}

export default function TimeSection() {
  return (
    <Section title="Time (MobX)">
      <Time />
    </Section>
  );
}

const Time = observer(() => {
  // const [{ time }] = useState(() => new TimeStore());
  const [{ time }] = useState(() => new TimeStoreWithDecorators());

  const loading = !time;
  const data = time;
  const error = false;
  const cancel = () => {};

  return loading ? (
    <>Loading...</>
  ) : error ? (
    <>There was an error!</>
  ) : (
    <div>
      {formatDate(new Date(data), "yyyy-MM-dd HH:mm:ss.SSS xxx")}
      <span className="inline-flex ml-3 align-text-top rounded-md shadow-sm">
        <button
          type="button"
          className="inline-flex items-center px-1 text-xs font-medium leading-4 text-white transition duration-150 ease-in-out bg-gray-600 border border-transparent rounded hover:bg-gray-500 focus:outline-none focus:border-gray-700 focus:shadow-outline-gray active:bg-gray-700"
          onClick={cancel}
        >
          Cancel Wait
        </button>
      </span>
    </div>
  );
});
