// SearchBar.tsx
// PRESENTATIONAL component — renders the Hotels.com-style search bar.
// Receives all state and callbacks from HomePage via props.

import { useState } from "react";
import type { HotelSearchRequest } from "../../types/hotel.types";
import "./SearchBar.css";

interface SearchBarProps {
  searchForm: HotelSearchRequest;
  onCityChange: (value: string) => void;
  onCheckInChange: (value: string) => void;
  onCheckOutChange: (value: string) => void;
  onAdultsChange: (value: number) => void;
  onChildrenChange: (value: number) => void;
  onRoomsChange: (value: number) => void;
  onSearch: () => void;
  isLoading: boolean;
}

function SearchBar(props: SearchBarProps) {
  // Controls whether the guests/rooms dropdown panel is visible
  const [isGuestPanelOpen, setIsGuestPanelOpen] = useState(false);

  // Helper: build the summary label shown in the guests button
  function buildGuestLabel(): string {
    const totalTravellers = props.searchForm.adults + props.searchForm.children;
    return `${totalTravellers} traveller${totalTravellers !== 1 ? "s" : ""}, ${props.searchForm.rooms} room${props.searchForm.rooms !== 1 ? "s" : ""}`;
  }

  // Helper: safely change a counter — clamps to [min, max]
  function changeCount(
    current: number,
    delta: number,
    min: number,
    max: number,
    onChange: (v: number) => void
  ) {
    const next = current + delta;
    if (next < min || next > max) return;
    onChange(next);
  }

  return (
    <div className="searchbar-wrapper">
      {/* ── Main search row ── */}
      <div className="searchbar-row">

        {/* WHERE TO */}
        <div className="searchbar-field searchbar-field--location">
          <span className="searchbar-icon">📍</span>
          <input
            className="searchbar-input"
            type="text"
            placeholder="Where to?"
            value={props.searchForm.cityName}
            onChange={(e) => props.onCityChange(e.target.value)}
          />
        </div>

        <div className="searchbar-divider" />

        {/* CHECK-IN */}
        <div className="searchbar-field searchbar-field--date">
          <span className="searchbar-icon">📅</span>
          <div className="searchbar-date-group">
            <label className="searchbar-date-label">Check-in</label>
            <input
              className="searchbar-input searchbar-input--date"
              type="date"
              value={props.searchForm.checkIn}
              onChange={(e) => props.onCheckInChange(e.target.value)}
            />
          </div>
        </div>

        <span className="searchbar-date-arrow">→</span>

        {/* CHECK-OUT */}
        <div className="searchbar-field searchbar-field--date">
          <div className="searchbar-date-group">
            <label className="searchbar-date-label">Check-out</label>
            <input
              className="searchbar-input searchbar-input--date"
              type="date"
              value={props.searchForm.checkOut}
              onChange={(e) => props.onCheckOutChange(e.target.value)}
            />
          </div>
        </div>

        <div className="searchbar-divider" />

        {/* GUESTS TRIGGER */}
        <div className="searchbar-field searchbar-field--guests" onClick={() => setIsGuestPanelOpen(!isGuestPanelOpen)}>
          <span className="searchbar-icon">👤</span>
          <span className="searchbar-guests-label">{buildGuestLabel()}</span>
          <span className="searchbar-chevron">{isGuestPanelOpen ? "▲" : "▼"}</span>
        </div>

        {/* SEARCH BUTTON */}
        <button
          className="searchbar-btn"
          onClick={props.onSearch}
          disabled={props.isLoading}
        >
          {props.isLoading ? "..." : "Search"}
        </button>
      </div>

      {/* ── Guests & Rooms dropdown panel ── */}
      {isGuestPanelOpen && (
        <div className="guest-panel">
          <div className="guest-panel__header">Guests &amp; Rooms</div>

          {/* Adults row */}
          <div className="guest-panel__row">
            <div>
              <div className="guest-panel__row-title">Adults</div>
            </div>
            <div className="guest-counter">
              <button
                className="guest-counter__btn"
                onClick={() => changeCount(props.searchForm.adults, -1, 1, 30, props.onAdultsChange)}
              >
                −
              </button>
              <span className="guest-counter__value">{props.searchForm.adults}</span>
              <button
                className="guest-counter__btn"
                onClick={() => changeCount(props.searchForm.adults, +1, 1, 30, props.onAdultsChange)}
              >
                +
              </button>
            </div>
          </div>

          {/* Children row */}
          <div className="guest-panel__row">
            <div>
              <div className="guest-panel__row-title">Children</div>
              <div className="guest-panel__row-sub">Ages 0 to 17</div>
            </div>
            <div className="guest-counter">
              <button
                className="guest-counter__btn"
                onClick={() => changeCount(props.searchForm.children, -1, 0, 10, props.onChildrenChange)}
              >
                −
              </button>
              <span className="guest-counter__value">{props.searchForm.children}</span>
              <button
                className="guest-counter__btn"
                onClick={() => changeCount(props.searchForm.children, +1, 0, 10, props.onChildrenChange)}
              >
                +
              </button>
            </div>
          </div>

          {/* Rooms row */}
          <div className="guest-panel__row">
            <div>
              <div className="guest-panel__row-title">Rooms</div>
            </div>
            <div className="guest-counter">
              <button
                className="guest-counter__btn"
                onClick={() => changeCount(props.searchForm.rooms, -1, 1, 9, props.onRoomsChange)}
              >
                −
              </button>
              <span className="guest-counter__value">{props.searchForm.rooms}</span>
              <button
                className="guest-counter__btn"
                onClick={() => changeCount(props.searchForm.rooms, +1, 1, 9, props.onRoomsChange)}
              >
                +
              </button>
            </div>
          </div>

          {/* Done button */}
          <button
            className="guest-panel__done"
            onClick={() => setIsGuestPanelOpen(false)}
          >
            Done
          </button>
        </div>
      )}
    </div>
  );
}

export default SearchBar;
