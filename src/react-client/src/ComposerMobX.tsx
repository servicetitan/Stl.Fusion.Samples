import React, { useState } from "react";
import { makeAutoObservable } from "mobx";
import { observer } from "mobx-react-lite";
import Section from "./Section";
import { fusion, makeFusionObservable } from "stl.fusion";

type MessageType = {
  id?: number;
  Id?: number;
  userId?: number;
  UserId?: number;
  text?: string;
  Text?: string;
  createdAt?: string;
  CreatedAt?: string;
};

type UserType = {
  id?: number;
  Id?: number;
  name?: string;
  Name?: string;
};

type ChatTailType = {
  messages?: MessageType[];
  Messages?: MessageType[];
  users?: {
    [id: number]: UserType;
  };
  Users?: {
    [id: number]: UserType;
  };
};

type SessionType = {
  id?: string;
  Id?: string;
};

type ComposerType = {
  parameter?: string;
  Parameter?: string;
  uptime?: string;
  Uptime?: string;
  lastChatMessage?: string;
  LastChatMessage?: string;
  user?: UserType;
  User?: UserType;
  activeUserCount?: number;
  ActiveUserCount?: number;
};

class ComposerLocalStore {
  @fusion("/api/Time/getUptime")
  time = "";

  @fusion("/api/Chat/getChatTail?length=1")
  chat = null as ChatTailType | null;

  @fusion("/fusion/auth/getUser")
  user = null as UserType | null;

  @fusion("/api/Chat/getActiveUserCount")
  userCount = 0;

  constructor() {
    makeAutoObservable(this);
    makeFusionObservable(this);
  }
}

class SessionStore {
  @fusion("/fusion/auth/getSessionInfo")
  session = null as SessionType | null;

  constructor() {
    makeAutoObservable(this);
    makeFusionObservable(this);
  }
}

function createComposerRemoteStore(parameter: string, sessionId: string) {
  console.log("createComposerRemoteStore");

  class ComposerRemoteStore {
    @fusion(
      `/api/composer/get?parameter=${parameter ?? ""}&session=${sessionId}`
    )
    data = null as ComposerType | null;

    constructor() {
      makeAutoObservable(this);
      makeFusionObservable(this);
    }
  }

  return new ComposerRemoteStore();
}

export default function ComposerSection() {
  const [parameter, setParameter] = useState("Parameter");

  return (
    <Section
      title="Composer (MobX)"
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

function Composer({ parameter }: { parameter: string }) {
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
          <SessionWrapper>
            {(sessionId: string) =>
              !sessionId ? (
                <>Loading...</>
              ) : (
                <ComposerRemote
                  parameter={parameter}
                  sessionId={sessionId}
                  onToggle={() => setShowRemote(!showRemote)}
                />
              )
            }
          </SessionWrapper>
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

const ComposerLocal = observer(
  ({ parameter, onToggle }: { parameter: string; onToggle: () => void }) => {
    const [{ time, chat, user, userCount }] = useState(
      () => new ComposerLocalStore()
    );

    return (
      <div>
        <h3 className="text-xs font-semibold leading-5 text-gray-600">
          Local - <button onClick={onToggle}>Hide</button>
        </h3>

        <div className="space-y-1 divide-y">
          <div>{parameter} - local</div>
          <div>{!time ? <>"Loading..."</> : time}</div>
          <div>
            {!chat ? (
              <>"Loading..."</>
            ) : (
              chat.messages?.[0]?.text ??
              chat.Messages?.[0]?.Text ??
              "(no messages)"
            )}
          </div>
          <div>{!user ? <>"Loading..."</> : user.id ?? user.Id}</div>
          <div>{!user ? <>"Loading..."</> : user.name ?? user.Name}</div>
          <div>
            {!userCount ? (
              <>"Loading..."</>
            ) : (
              `${userCount} active
      user${userCount === 1 ? "" : "s"}`
            )}
          </div>
        </div>
      </div>
    );
  }
);

const SessionWrapper = observer(({ children }: any) => {
  const [{ session }] = useState(() => new SessionStore());
  const sessionId = session ? session?.id ?? session?.Id : null;
  return children(sessionId);
});

const ComposerRemote = observer(
  ({
    parameter,
    sessionId,
    onToggle,
  }: {
    parameter: string;
    sessionId: string;
    onToggle: () => void;
  }) => {
    const [{ data }] = useState(() =>
      createComposerRemoteStore(parameter, sessionId)
    );

    return (
      <div>
        <h3 className="text-xs font-semibold leading-5 text-gray-600">
          Remote - <button onClick={onToggle}>Hide</button>
        </h3>

        {!data ? (
          <>"Loading..."</>
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
);
