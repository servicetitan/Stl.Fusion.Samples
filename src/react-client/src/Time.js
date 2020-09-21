import React from "react";
import formatDate from "date-fns/format";
import Section from "./Section";
import useStlFusion from "./lib/useStlFusion";

export default function Time() {
  return (
    <Section title="Time">
      <TimeSubscription />
    </Section>
  );
}

function TimeSubscription() {
  const { data, loading, error } = useStlFusion("/api/Time/get");

  return loading
    ? "Loading..."
    : error
    ? "There was an error!"
    : formatDate(new Date(data), "yyyy-MM-dd HH:mm:ss.SSS xxx");
}
