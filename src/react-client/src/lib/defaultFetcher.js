export default async function fetcher(url, options) {
  let optionsWithFusionHeader =
    options == null
      ? { headers: { "x-fusion-publish": 1 } }
      : {
          ...options,
          headers: { ...options.headers, "x-fusion-publish": 1 },
        };

  const res = await fetch(url, optionsWithFusionHeader);

  if (!res.ok) {
    throw new Error("Network response was not ok");
  }

  // TODO: Should the fetcher really be handling all this header-parsing logic?
  const headers = JSON.parse(res.headers.get("x-fusion-publication"));

  if (!headers) {
    throw new Error("STL.Fusion publication header was not in response");
  }

  return {
    data: await res.json(),
    headers: {
      PublisherId: headers.PublicationRef.PublisherId,
      PublicationId: headers.PublicationRef.PublicationId,
      Version: headers.Version,
      IsConsistent: headers.IsConsistent,
    },
  };
}
