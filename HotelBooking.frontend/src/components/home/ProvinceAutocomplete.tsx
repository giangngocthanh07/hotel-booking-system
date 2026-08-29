import { useMemo, useState } from "react";
import type { FocusEvent, KeyboardEvent } from "react";
import type { Province } from "../../types/hotel.types";
import { filterProvinces } from "../../utils/provinceSearch";

interface ProvinceAutocompleteProps {
  value: string;
  provinces: Province[];
  selectedProvince: Province | null;
  onInputChange: (value: string) => void;
  onSelect: (province: Province) => void;
  isLoading: boolean;
  error?: string;
  onRetry?: () => void;
}

const LISTBOX_ID = "province-options";

function ProvinceAutocomplete(props: ProvinceAutocompleteProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);
  const options = useMemo(
    () => filterProvinces(props.provinces, props.value),
    [props.provinces, props.value],
  );

  function selectProvince(province: Province) {
    props.onSelect(province);
    setIsOpen(false);
    setActiveIndex(-1);
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Escape") return closeOptions();
    if (!isOpen || options.length === 0) return;

    if (event.key === "ArrowDown") {
      event.preventDefault();
      setActiveIndex((current) => (current + 1) % options.length);
    } else if (event.key === "ArrowUp") {
      event.preventDefault();
      setActiveIndex((current) =>
        current <= 0 ? options.length - 1 : current - 1,
      );
    } else if (event.key === "Enter" && activeIndex >= 0) {
      event.preventDefault();
      selectProvince(options[activeIndex]);
    }
  }

  function closeOptions() {
    setIsOpen(false);
    setActiveIndex(-1);
  }

  function handleBlur(event: FocusEvent<HTMLDivElement>) {
    const nextTarget = event.relatedTarget;
    if (nextTarget instanceof Node && event.currentTarget.contains(nextTarget)) {
      return;
    }
    closeOptions();
  }

  return (
    <div className="province-autocomplete" onBlur={handleBlur}>
      <input
        className="searchbar-input"
        type="text"
        role="combobox"
        aria-label="Province or city"
        aria-expanded={isOpen}
        aria-controls={LISTBOX_ID}
        aria-autocomplete="list"
        aria-activedescendant={
          activeIndex >= 0 ? `province-option-${options[activeIndex].id}` : undefined
        }
        placeholder={props.isLoading ? "Loading destinations..." : "Where to?"}
        value={props.value}
        onChange={(event) => {
          props.onInputChange(event.target.value);
          setIsOpen(true);
          setActiveIndex(-1);
        }}
        onFocus={() => setIsOpen(true)}
        onKeyDown={handleKeyDown}
        autoComplete="off"
      />

      {isOpen && (
        <div className="province-options" id={LISTBOX_ID} role="listbox">
          {props.isLoading && (
            <div className="province-empty" role="status">
              Loading destinations...
            </div>
          )}
          {!props.isLoading && props.error && (
            <div className="province-error" role="alert">
              <span>{props.error}</span>
              {props.onRetry && (
                <button
                  className="province-retry"
                  type="button"
                  onClick={props.onRetry}
                >
                  Retry
                </button>
              )}
            </div>
          )}
          {!props.isLoading && !props.error && options.length === 0 && (
            <div className="province-empty">No destinations found.</div>
          )}
          {!props.isLoading &&
            !props.error &&
            options.map((province, index) => (
              <button
                className={`province-option${index === activeIndex ? " province-option--active" : ""}`}
                id={`province-option-${province.id}`}
                key={province.id}
                type="button"
                role="option"
                aria-selected={props.selectedProvince?.id === province.id}
                onMouseDown={(event) => event.preventDefault()}
                onClick={() => selectProvince(province)}
              >
                <span aria-hidden="true">📍</span>
                <span>{province.name}</span>
              </button>
            ))}
        </div>
      )}
    </div>
  );
}

export default ProvinceAutocomplete;
