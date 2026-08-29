import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import ProvinceAutocomplete from "./ProvinceAutocomplete";

const provinces = [
  { id: 1, name: "Đà Nẵng" },
  { id: 2, name: "Hà Nội" },
];

describe("ProvinceAutocomplete", () => {
  it("filters without accents and selects a suggestion", async () => {
    const user = userEvent.setup();
    const onInputChange = vi.fn();
    const onSelect = vi.fn();
    const view = render(
      <ProvinceAutocomplete
        value=""
        provinces={provinces}
        selectedProvince={null}
        onInputChange={onInputChange}
        onSelect={onSelect}
        isLoading={false}
      />,
    );

    await user.click(screen.getByRole("combobox"));
    await user.type(screen.getByRole("combobox"), "da nang");
    view.rerender(
      <ProvinceAutocomplete
        value="da nang"
        provinces={provinces}
        selectedProvince={null}
        onInputChange={onInputChange}
        onSelect={onSelect}
        isLoading={false}
      />,
    );
    await user.click(screen.getByRole("option", { name: "Đà Nẵng" }));

    expect(onSelect).toHaveBeenCalledWith(provinces[0]);
  });

  it("selects the highlighted suggestion with the keyboard", async () => {
    const user = userEvent.setup();
    const onSelect = vi.fn();
    render(
      <ProvinceAutocomplete
        value=""
        provinces={provinces}
        selectedProvince={null}
        onInputChange={vi.fn()}
        onSelect={onSelect}
        isLoading={false}
      />,
    );

    await user.click(screen.getByRole("combobox"));
    await user.keyboard("{ArrowDown}{Enter}");

    expect(onSelect).toHaveBeenCalledWith(provinces[0]);
  });
});
