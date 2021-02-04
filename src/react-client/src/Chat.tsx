import React, { useState, useEffect, useCallback, useRef } from "react";
import debounce from "lodash/debounce";
import formatDate from "date-fns/format";
import Section from "./Section";
import { useFusionSubscription } from "stl.fusion";
import LoadingSVG from "./LoadingSVG";

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

export default function Chat() {
  const [user, setUser] = useState(null);
  const [cancel, setCancel] = useState(null);

  return (
    <Section
      title="Chat"
      header={<ChatUser user={user} onUserChange={setUser} />}
      footer={<AddChatMessage user={user} cancel={cancel} />}
    >
      <ChatMessages onCancelChange={setCancel} />
    </Section>
  );
}

function ChatMessages({
  onCancelChange,
}: {
  onCancelChange: React.Dispatch<React.SetStateAction<any>>;
}) {
  const {
    data: activeUserData,
    loading: activeUserLoading,
    error: activeUserError,
  } = useFusionSubscription(
    null as number | null,
    "/api/Chat/getActiveUserCount"
  );

  const activeUserCount =
    !activeUserLoading && !activeUserError ? activeUserData : 0;

  const { data, loading, error, cancel } = useFusionSubscription(
    null as ChatTailType | null,
    "/api/Chat/getChatTail?length=5"
  );

  useEffect(() => {
    onCancelChange(() => cancel);
  }, [onCancelChange, cancel]);

  const messages = data?.messages ?? data?.Messages ?? [];
  const users = data?.users ?? data?.Users ?? {};

  return loading ? (
    <>"Loading..."</>
  ) : error ? (
    <>"There was an error!"</>
  ) : (
    <div className="relative py-2 space-y-2">
      <div className="absolute top-0 right-0 text-xs font-semibold leading-5 text-gray-600">
        {`${activeUserCount} active user${activeUserCount === 1 ? "" : "s"}`}
      </div>

      {messages.length ? (
        messages.map((message) => {
          const messageId = message.id ?? message.Id;
          const userId = message.userId ?? message.UserId ?? 1;
          const name = users[userId].name ?? users[userId].Name ?? "";
          const createdAt = message.createdAt ?? message.CreatedAt ?? "";

          return (
            <div key={messageId} className="flex items-center">
              <div className="flex-shrink-0">
                <div className="inline-block w-10 h-10 font-extrabold leading-10 text-center text-gray-600 bg-gray-200 rounded-md shadow-inner">
                  {name[0].toUpperCase()}
                </div>
              </div>
              <div className="ml-3">
                <p className="text-xs font-semibold leading-5 text-gray-600">
                  {name} • {formatDate(new Date(createdAt), "h:mm a")}
                </p>
                <p className="leading-5">{message.text ?? message.Text}</p>
              </div>
            </div>
          );
        })
      ) : (
        <div className="flex items-center text-lg">No messages yet!</div>
      )}
    </div>
  );
}

function ChatUser({
  user,
  onUserChange,
}: {
  user: UserType | null;
  onUserChange: React.Dispatch<React.SetStateAction<any>>;
}) {
  const [loading, setLoading] = useState(false);
  const inputRef = useRef(null as HTMLInputElement | null);

  const newUser = useCallback(() => {
    setLoading(true);

    fetch(`/api/Chat/createUser`, {
      method: "POST",
    })
      .then((res) => res.json())
      .then((data) => {
        onUserChange(data);
        if (inputRef.current != null) {
          inputRef.current.value = data ? data.name ?? data.Name : "";
        }
        setLoading(false);
      })
      .catch((err) => {
        setLoading(false);
        throw err;
      });
  }, [onUserChange]);

  useEffect(() => {
    setLoading(true);

    fetch(`/api/Chat/getUser?id=1`)
      .then((res) => res.json())
      .then((data) => {
        onUserChange(data);
        setLoading(false);
      })
      .catch((err) => {
        newUser();
      });
  }, [onUserChange, newUser]);

  const debouncedSetUserName = debounce((name) => {
    if (user == null) {
      return;
    }

    setLoading(true);

    fetch(`/api/Chat/setUserName?id=${user.id ?? user.Id}&name=${name}`, {
      method: "POST",
    })
      .then((res) => res.json())
      .then((data) => {
        // console.log("setUserName", data);
        onUserChange(data);
        setLoading(false);
      })
      .catch((err) => {
        throw err;
      });
  }, 300);

  return (
    <>
      <span className="inline-flex align-text-top rounded-md shadow-sm">
        <button
          type="button"
          className="inline-flex items-center px-1 text-xs font-medium leading-4 text-white transition duration-150 ease-in-out bg-gray-600 border border-transparent rounded hover:bg-gray-500 focus:outline-none focus:border-gray-700 focus:shadow-outline-gray active:bg-gray-700"
          onClick={newUser}
        >
          New User
        </button>
      </span>{" "}
      <span className="inline-block ml-3 text-sm font-normal align-text-bottom">
        You are:
      </span>{" "}
      <input
        ref={inputRef}
        className="px-2 py-1 text-xs leading-5 align-bottom transition duration-150 ease-in-out bg-gray-200 border border-gray-300 rounded"
        defaultValue={user ? user.name ?? user.Name : ""}
        onChange={({ target: { value } }) => debouncedSetUserName(value)}
      />
      {loading ? (
        <>
          {" "}
          <LoadingSVG className="inline-block align-text-top" />
        </>
      ) : null}
    </>
  );
}

function AddChatMessage({
  user,
  cancel,
}: {
  user: UserType | null;
  cancel: (() => void) | null;
}) {
  const [text, setText] = useState("");
  const [loading, setLoading] = useState(false);
  const [shouldCancel, setShouldCancel] = useState(true);

  function addMessage() {
    if (user == null) {
      return;
    }

    setLoading(true);
    fetch(`/api/Chat/addMessage?userId=${user.id}&text=${text}`, {
      method: "POST",
    })
      .then((res) => res.json())
      .then((data) => {
        if (shouldCancel && cancel) {
          cancel();
        }
        setText("");
        setLoading(false);
      })
      .catch((err) => {
        throw err;
      });
  }

  return (
    <div>
      <form
        onSubmit={(e: React.ChangeEvent<HTMLFormElement>) => {
          e.preventDefault();
          addMessage();
          e.target.reset();
        }}
      >
        <div className="flex py-2">
          <div className="relative flex-grow focus-within:z-10">
            {loading ? (
              <LoadingSVG
                className="absolute"
                style={{
                  top: "calc(1.25rem - 11px)",
                  left: "50%",
                  marginLeft: "-11px",
                }}
              />
            ) : null}

            <input
              className="block w-full h-10 pl-3 text-sm leading-5 transition duration-150 ease-in-out bg-gray-200 border border-gray-300 rounded-none rounded-l-md"
              placeholder={`Send chat message as ${user?.name ?? user?.Name}`}
              onChange={({ target: { value } }) => setText(value)}
            />
          </div>
          <button className="relative inline-flex items-center px-4 py-2 -ml-px text-sm font-medium leading-5 text-gray-700 transition duration-150 ease-in-out border border-gray-300 rounded-r-md bg-gray-50 hover:text-gray-500 hover:bg-white focus:outline-none focus:shadow-outline-blue focus:border-blue-300 active:bg-gray-100 active:text-gray-700">
            <span>Submit</span>
          </button>
        </div>
      </form>

      <div>
        <label className="text-xs font-semibold leading-5 text-gray-600">
          <input
            type="checkbox"
            checked={shouldCancel}
            onChange={({ target: { checked } }) => {
              setShouldCancel(checked);
            }}
          />{" "}
          <span className="align-text-bottom">
            Cancel wait when sending message
          </span>
        </label>
      </div>
    </div>
  );
}
