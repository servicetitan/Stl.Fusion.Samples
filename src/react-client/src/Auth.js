import React from "react";
import Section from "./Section";
import useFusionSubscription from "./lib/useFusionSubscription";

export default function AuthSection() {
  return (
    <Section title="Auth">
      <Auth />
    </Section>
  );
}

function Auth() {
  const {
    data: sessionData,
    loading: sessionLoading,
    error: sessionError,
  } = useFusionSubscription("/fusion/auth/getSessionInfo");
  const {
    data: userData,
    loading: userLoading,
    error: userError,
  } = useFusionSubscription("/fusion/auth/getUser");

  return (
    <div className="mt-2">
      <div className="grid max-w-5xl grid-cols-2 gap-5 mx-auto space-y-0">
        <div className="overflow-hidden">
          <h3 className="text-xs font-semibold leading-5 text-gray-600">
            Session
          </h3>

          {sessionLoading ? (
            "Loading..."
          ) : sessionError ? (
            "There was an error!"
          ) : (
            <div>
              <xmp>{JSON.stringify(sessionData, null, 2)}</xmp>
            </div>
          )}
        </div>

        <div className="overflow-hidden">
          <h3 className="text-xs font-semibold leading-5 text-gray-600">
            User
          </h3>

          {userLoading ? (
            "Loading..."
          ) : userError ? (
            "There was an error!"
          ) : (
            <div>
              <xmp>{JSON.stringify(userData, null, 2)}</xmp>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
