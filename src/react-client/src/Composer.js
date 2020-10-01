import React, { useState } from "react";
import Section from "./Section";
import useStlFusion from "./lib/useStlFusion";

export default function ComposerSection() {
  const [parameter, setParameter] = useState("Parameter");

  return (
    <Section
      title="Composer"
      header={
        <>
          <span className="inline-block ml-3 text-sm font-normal align-text-bottom">
            Parameter:
          </span>{" "}
          <input
            className="px-2 py-1 text-xs leading-5 align-bottom transition duration-150 ease-in-out bg-gray-200 border border-gray-300 rounded"
            defaultValue={parameter}
            onChange={({ target: { value } }) => setParameter(value)}
          />
        </>
      }
    >
      <Composer parameter={parameter} />
    </Section>
  );
}

function Composer({ parameter }) {
  const [showLocal, setShowLocal] = useState(true);
  const [showRemote, setShowRemote] = useState(true);

  return (
    <div className="mt-2">
      <div className="grid max-w-5xl grid-cols-2 gap-5 mx-auto space-y-0">
        {showLocal ? (
          <ComposerLocal
            parameter={parameter}
            onToggle={() => setShowLocal(!showLocal)}
          />
        ) : (
          <div>
            <button
              className="text-xs font-semibold leading-5 text-gray-600"
              onClick={() => setShowLocal(!showLocal)}
            >
              Show Local
            </button>
          </div>
        )}

        {showRemote ? (
          <ComposerRemote
            parameter={parameter}
            onToggle={() => setShowRemote(!showRemote)}
          />
        ) : (
          <div>
            <button
              className="text-xs font-semibold leading-5 text-gray-600"
              onClick={() => setShowRemote(!showRemote)}
            >
              Show Remote
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

function ComposerLocal({ parameter, onToggle }) {
  const {
    data: timeData,
    loading: timeLoading,
    error: timeError,
  } = useStlFusion("/api/Time/getUptime");
  const {
    data: chatData,
    loading: chatLoading,
    error: chatError,
  } = useStlFusion("/api/Chat/getChatTail?length=1");
  const {
    data: userData,
    loading: userLoading,
    error: userError,
  } = useStlFusion("/fusion/auth/getUser");
  const {
    data: userCountData,
    loading: userCountLoading,
    error: userCountError,
  } = useStlFusion("/api/Chat/getActiveUserCount");

  return (
    <div>
      <h3 className="text-xs font-semibold leading-5 text-gray-600">
        Local - <button onClick={onToggle}>Hide</button>
      </h3>

      <div className="space-y-1 divide-y">
        <div>{parameter} - local</div>
        <div>
          {timeLoading
            ? "Loading..."
            : timeError
            ? "There was an error!"
            : timeData}
        </div>
        <div>
          {chatLoading
            ? "Loading..."
            : chatError
            ? "There was an error!"
            : chatData.messages?.[0]?.text ??
              chatData.Messages?.[0]?.Text ??
              "(no messages)"}
        </div>
        <div>
          {userLoading
            ? "Loading..."
            : userError
            ? "There was an error!"
            : userData.id ?? userData.Id}
        </div>
        <div>
          {userLoading
            ? "Loading..."
            : userError
            ? "There was an error!"
            : userData.name ?? userData.Name}
        </div>
        <div>
          {userCountLoading
            ? "Loading..."
            : userCountError
            ? "There was an error!"
            : `${userCountData} active
      user${userCountData === 1 ? "" : "s"}`}
        </div>
      </div>
    </div>
  );
}

function ComposerRemote({ parameter, onToggle }) {
  const {
    data: sessionData,
    loading: sessionLoading,
    error: sessionError,
  } = useStlFusion("/fusion/auth/getSessionInfo");

  const sessionId =
    !sessionLoading && !sessionError ? sessionData.id ?? sessionData.Id : null;

  const { data, loading, error } = useStlFusion(
    sessionId
      ? `/api/composer/get?parameter=${parameter ?? ""}&session=${sessionId}`
      : null
  );

  return (
    <div>
      <h3 className="text-xs font-semibold leading-5 text-gray-600">
        Remote - <button onClick={onToggle}>Hide</button>
      </h3>

      {loading ? (
        "Loading..."
      ) : error ? (
        "There was an error!"
      ) : (
        <div className="space-y-1 divide-y">
          <div>{data.parameter ?? data.Parameter}</div>
          <div>{data.uptime ?? data.Uptime}</div>
          <div>{data.lastChatMessage ?? data.LastChatMessage}</div>
          <div>{data?.user?.id ?? data?.User?.Id}</div>
          <div>{data?.user?.name ?? data?.User?.Name}</div>
          <div>
            {data.activeUserCount ?? data.ActiveUserCount} active user
            {data.activeUserCount ?? data.ActiveUserCount === 1 ? "" : "s"}
          </div>
        </div>
      )}
    </div>
  );
}
