export default async function fetcher(url, options) {
  const res = await fetch(url, {
    ...options,
    headers: { ...options?.headers, "x-fusion-publish": 1 },
  });

  if (!res.ok) {
    throw new Error("Network response was not ok");
  }

  const header = JSON.parse(res.headers.get("x-fusion-publication"));

  if (!header) {
    throw new Error("STL.Fusion publication header was not in response");
  }

  return { data: await res.json(), header };
}
