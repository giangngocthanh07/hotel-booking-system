import { useEffect, useState } from "react";

interface Result<T> { key: string; data: T | null; error: string; loading: boolean }

export default function useOwnerHotelData<T>(key: string, load: (signal: AbortSignal) => Promise<T>) {
  const [attempt, setAttempt] = useState(0);
  const [result, setResult] = useState<Result<T>>({ key, data: null, error: "", loading: true });
  useEffect(() => {
    const controller = new AbortController();
    async function run() {
      setResult({ key, data: null, error: "", loading: true });
      try {
        const data = await load(controller.signal);
        if (!controller.signal.aborted) setResult({ key, data, error: "", loading: false });
      } catch (failure) {
        if (!controller.signal.aborted) setResult({ key, data: null, loading: false,
          error: failure instanceof Error ? failure.message : "Unable to load hotel information." });
      }
    }
    void run();
    return () => controller.abort();
  }, [key, load, attempt]);
  // FR-CAT-003: hide previous Hotel/page data immediately, even before the new effect runs.
  const current = result.key === key ? result : { data: null, error: "", loading: true };
  return { ...current, retry: () => setAttempt(value => value + 1) };
}
