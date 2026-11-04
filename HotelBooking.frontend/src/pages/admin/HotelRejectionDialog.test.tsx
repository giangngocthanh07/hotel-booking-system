import { act, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import HotelRejectionDialog from "./HotelRejectionDialog";

describe("HotelRejectionDialog (FR-ONB-017)", () => {
  it.each(["", "           ", "123456789", "x".repeat(501)])(
    "blocks invalid explanations without calling the API: %s", async (reason) => {
      const user = userEvent.setup();
      const submit = vi.fn();
      render(<HotelRejectionDialog onSubmit={submit} onClose={vi.fn()} />);
      if (reason) {
        await user.click(screen.getByRole("textbox", { name: "Rejection reason" }));
        await user.paste(reason);
      }
      await user.click(screen.getByRole("button", { name: "Confirm rejection" }));
      expect(submit).not.toHaveBeenCalled();
      expect(screen.getByRole("alert")).toHaveTextContent("10–500");
    },
  );

  it.each([10, 500])("submits the trimmed explanation at the %i-character boundary", async (length) => {
    const user = userEvent.setup();
    const submit = vi.fn().mockResolvedValue(undefined);
    render(<HotelRejectionDialog onSubmit={submit} onClose={vi.fn()} />);
    const reason = "x".repeat(length);
    await user.click(screen.getByRole("textbox", { name: "Rejection reason" }));
    await user.paste(`  ${reason}  `);
    await user.click(screen.getByRole("button", { name: "Confirm rejection" }));
    await waitFor(() => expect(submit).toHaveBeenCalledWith(reason));
  });

  it("retains the explanation and shows the server error so the Admin can retry", async () => {
    const user = userEvent.setup();
    const submit = vi.fn().mockRejectedValue(new Error("Request is no longer pending."));
    render(<HotelRejectionDialog onSubmit={submit} onClose={vi.fn()} />);
    await user.type(screen.getByRole("textbox", { name: "Rejection reason" }), "License is incomplete.");
    await user.click(screen.getByRole("button", { name: "Confirm rejection" }));
    expect(await screen.findByRole("alert")).toHaveTextContent("Request is no longer pending.");
    expect(screen.getByRole("textbox", { name: "Rejection reason" })).toHaveValue("License is incomplete.");
    expect(screen.getByRole("button", { name: "Confirm rejection" })).toBeEnabled();
  });

  it("cancels without sending a rejection", async () => {
    const user = userEvent.setup();
    const close = vi.fn();
    const submit = vi.fn();
    render(<HotelRejectionDialog onSubmit={submit} onClose={close} />);
    await user.click(screen.getByRole("button", { name: "Cancel" }));
    expect(close).toHaveBeenCalledOnce();
    expect(submit).not.toHaveBeenCalled();
  });

  it("prevents duplicate submission and closing while the decision is being saved", async () => {
    const user = userEvent.setup();
    let finish!: () => void;
    const pending = new Promise<void>((resolve) => { finish = resolve; });
    const submit = vi.fn().mockReturnValue(pending);
    render(<HotelRejectionDialog onSubmit={submit} onClose={vi.fn()} />);
    await user.type(screen.getByRole("textbox", { name: "Rejection reason" }), "License is incomplete.");
    await user.dblClick(screen.getByRole("button", { name: "Confirm rejection" }));
    expect(submit).toHaveBeenCalledOnce();
    expect(screen.getByRole("button", { name: "Rejecting…" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Cancel" })).toBeDisabled();
    expect(screen.getByRole("textbox", { name: "Rejection reason" })).toBeDisabled();
    await act(async () => { finish(); await pending; });
  });
});
