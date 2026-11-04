import { useState } from "react";
import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, TextField } from "@mui/material";

interface Props {
  requestId?: number;
  title?: string;
  description?: string;
  onSubmit: (reason: string) => Promise<void>;
  onClose: () => void;
}

// Keep the explanation and recoverable errors local; the page owns API calls and list refreshes.
export default function HotelRejectionDialog({ requestId, title = "Reject hotel registration",
  description = "Explain why this registration cannot be approved. This decision will reject the request.", onSubmit, onClose }: Props) {
  const [reason, setReason] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  async function submit(event: React.FormEvent) {
    event.preventDefault();
    if (saving) return;
    const explanation = reason.trim();
    // Match backend validation so whitespace cannot satisfy the required explanation length.
    if (explanation.length < 10 || explanation.length > 500) {
      setError("Rejection reason must contain 10–500 characters after trimming.");
      return;
    }
    setSaving(true);
    setError("");
    try {
      await onSubmit(explanation);
    } catch (failure) {
      setError(failure instanceof Error ? failure.message : "Unable to reject this request. Please try again.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog open fullWidth maxWidth="sm" aria-labelledby="hotel-rejection-title"
      onClose={() => { if (!saving) onClose(); }}>
      <form onSubmit={submit} noValidate aria-busy={saving}>
        <DialogTitle id="hotel-rejection-title">{title}</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {requestId !== undefined && `Request #${requestId}. `}
            {description}
          </DialogContentText>
          <TextField autoFocus fullWidth multiline minRows={4} margin="normal" required
            label="Rejection reason" value={reason} disabled={saving}
            onChange={(event) => { setReason(event.target.value); setError(""); }}
            helperText={`${reason.trim().length}/500 characters. Minimum 10 characters.`} />
          {error && <Alert severity="error" role="alert">{error}</Alert>}
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={saving}>Cancel</Button>
          <Button type="submit" color="error" variant="contained" disabled={saving}>
            {saving ? "Rejecting…" : "Confirm rejection"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
