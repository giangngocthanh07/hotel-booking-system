import { useCallback, useEffect, useRef, useState } from "react";
import { downloadBusinessLicense } from "../../services/businessLicenseService";

interface OpenDocument { tab: Window; abort: AbortController; url?: string; timer?: number }
interface Props { requestId: number; className?: string; label?: string }

export default function BusinessLicenseButton(props: Props) {
  // FR-ONB-013/014: switching requests must abort the previous file and reset its loading/error state.
  return <BusinessLicenseAction key={props.requestId} {...props} />;
}

function BusinessLicenseAction({ requestId, className, label = "View License" }: Props) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const active = useRef<OpenDocument | null>(null);

  const release = useCallback(() => {
    const document = active.current;
    if (!document) return;
    active.current = null;
    document.abort.abort();
    window.clearInterval(document.timer);
    if (document.url) URL.revokeObjectURL(document.url);
    else document.tab.close();
  }, []);

  useEffect(() => () => release(), [requestId, release]);

  async function openLicense() {
    if (active.current && !active.current.tab.closed) { active.current.tab.focus(); return; }
    release();
    setError("");
    // Open during the click gesture so browsers do not block the tab after the authenticated fetch.
    const tab = window.open("", "_blank");
    if (!tab) { setError("Allow pop-ups to view the business license, then try again."); return; }
    tab.opener = null;
    const document: OpenDocument = { tab, abort: new AbortController() };
    active.current = document;
    setLoading(true);
    document.timer = window.setInterval(() => {
      if (tab.closed) { release(); setLoading(false); }
    }, 1000);
    await loadDocument(document);
  }

  async function loadDocument(document: OpenDocument) {
    try {
      const blob = await downloadBusinessLicense(requestId, document.abort.signal);
      if (document.abort.signal.aborted || document.tab.closed) return;
      document.url = URL.createObjectURL(blob);
      document.tab.location.replace(document.url);
    } catch (failure) {
      if (!document.abort.signal.aborted) {
        setError(failure instanceof Error ? failure.message : "Unable to load the business license.");
        document.tab.close();
        release();
        setLoading(false);
      }
    } finally {
      if (active.current === document) setLoading(false);
    }
  }

  return <span>
    <button type="button" className={className} onClick={() => void openLicense()} disabled={loading} aria-busy={loading}>
      {loading ? "Loading license…" : label}
    </button>
    {error && <span role="alert"> {error}</span>}
  </span>;
}
