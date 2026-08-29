import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, useLocation } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { getVietnamProvinces } from "../services/locationService";
import HomePage from "./HomePage";

vi.mock("../services/locationService", () => ({
  getVietnamProvinces: vi.fn(),
}));

const mockedGetVietnamProvinces = vi.mocked(getVietnamProvinces);

function LocationProbe() {
  const location = useLocation();
  return <output data-testid="location">{location.pathname + location.search}</output>;
}

function renderHome() {
  return render(
    <MemoryRouter initialEntries={["/"]}>
      <HomePage />
      <LocationProbe />
    </MemoryRouter>,
  );
}

describe("HomePage province search", () => {
  beforeEach(() => {
    mockedGetVietnamProvinces.mockResolvedValue([
      { id: 1, name: "Đà Nẵng" },
      { id: 2, name: "Hà Nội" },
    ]);
  });

  it("navigates with the selected province and every search criterion", async () => {
    const user = userEvent.setup();
    renderHome();

    await user.click(await screen.findByRole("combobox"));
    await user.type(screen.getByRole("combobox"), "da nang");
    await user.click(screen.getByRole("option", { name: "Đà Nẵng" }));
    await user.click(screen.getByRole("button", { name: "Search" }));

    const location = screen.getByTestId("location").textContent ?? "";
    expect(location).toContain("/search-results?");
    expect(location).toContain("cityName=%C4%90%C3%A0+N%E1%BA%B5ng");
    expect(location).toMatch(/checkIn=\d{4}-\d{2}-\d{2}/);
    expect(location).toMatch(/checkOut=\d{4}-\d{2}-\d{2}/);
    expect(location).toContain("adults=1&children=0&rooms=1");
  });

  it("rejects unmatched free text", async () => {
    const user = userEvent.setup();
    renderHome();

    const input = await screen.findByRole("combobox");
    await user.type(input, "Không tồn tại");
    await user.click(screen.getByRole("button", { name: "Search" }));

    expect(screen.getByTestId("location")).toHaveTextContent("/");
    expect(
      screen.getByText("Vui lòng chọn một tỉnh/thành trong danh sách."),
    ).toBeInTheDocument();
  });
});
