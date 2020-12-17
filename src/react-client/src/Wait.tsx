import React from "react";
import Section from "./Section";

export default function Wait({
  wait,
  onWaitChange,
}: {
  wait: number;
  onWaitChange: React.Dispatch<React.SetStateAction<number>>;
}) {
  return (
    <Section
      title="Wait"
      header={
        <>
          <input
            name="composer-parameter"
            className="w-16 px-2 py-1 text-xs leading-5 text-center align-bottom transition duration-150 ease-in-out bg-gray-200 border border-gray-300 rounded"
            defaultValue={wait}
            onChange={({ target: { value } }) => {
              const num = Number(value);
              if (!isNaN(num) && num >= 10) {
                onWaitChange(num);
              }
            }}
          />{" "}
          <span className="inline-block text-sm font-normal align-text-bottom">
            milliseconds
          </span>
        </>
      }
    />
  );
}
