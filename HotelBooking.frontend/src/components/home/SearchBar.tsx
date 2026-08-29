// SearchBar.tsx
// PRESENTATIONAL component - Hotels.com style search bar.
// Uses TWO MUI DateCalendar side-by-side to mimic a date range picker (free tier).

import { useState } from "react";
import type { HotelSearchRequest } from "../../types/hotel.types";
import "./SearchBar.css";

import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { DateCalendar } from "@mui/x-date-pickers/DateCalendar";
import { PickersDay } from "@mui/x-date-pickers/PickersDay";
import type { PickersDayProps } from "@mui/x-date-pickers/PickersDay";
import dayjs from "dayjs";
import type { Dayjs } from "dayjs";

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
  const [isCalendarOpen, setIsCalendarOpen] = useState(false);
  const [isGuestPanelOpen, setIsGuestPanelOpen] = useState(false);

  // Which click are we waiting for: "start" or "end"
  const [selecting, setSelecting] = useState<"start" | "end">("start");

  // Month shown on left calendar (right is always +1)
  const [leftMonth, setLeftMonth] = useState<Dayjs>(dayjs().startOf("month"));

  const checkIn = props.searchForm.checkIn ? dayjs(props.searchForm.checkIn) : null;
  const checkOut = props.searchForm.checkOut ? dayjs(props.searchForm.checkOut) : null;

  // Date trigger label
  function buildDateLabel(): string {
    if (!checkIn && !checkOut) return "Select dates";
    const fmt = (d: Dayjs) => d.format("D MMM");
    const inStr = checkIn ? fmt(checkIn) : "?";
    const outStr = checkOut ? fmt(checkOut) : "?";
    return `${inStr}  →  ${outStr}`;
  }

  // Guest summary label
  function buildGuestLabel(): string {
    const total = props.searchForm.adults + props.searchForm.children;
    return `${total} traveller${total !== 1 ? "s" : ""}, ${props.searchForm.rooms} room${props.searchForm.rooms !== 1 ? "s" : ""}`;
  }

  // Handle day click — first click = check-in, second = check-out
  function handleDayClick(day: Dayjs) {
    if (selecting === "start") {
      props.onCheckInChange(day.format("YYYY-MM-DD"));
      props.onCheckOutChange("");
      setSelecting("end");
    } else {
      // If user picks an end date before start, swap them
      if (checkIn && day.isBefore(checkIn)) {
        props.onCheckOutChange(checkIn.format("YYYY-MM-DD"));
        props.onCheckInChange(day.format("YYYY-MM-DD"));
      } else {
        props.onCheckOutChange(day.format("YYYY-MM-DD"));
      }
      setSelecting("start");
      setIsCalendarOpen(false);
    }
  }

  // Determine if a day is within the selected range (for highlight)
  function isInRange(day: Dayjs): boolean {
    if (!checkIn || !checkOut) return false;
    return day.isAfter(checkIn) && day.isBefore(checkOut);
  }

  function isStart(day: Dayjs): boolean {
    return checkIn ? day.isSame(checkIn, "day") : false;
  }

  function isEnd(day: Dayjs): boolean {
    return checkOut ? day.isSame(checkOut, "day") : false;
  }

  // Custom day renderer: adds range-highlight classes
  function CustomDay(dayProps: PickersDayProps<Dayjs>) {
    const { day, outsideCurrentMonth, ...rest } = dayProps;
    const inRange = !outsideCurrentMonth && isInRange(day);
    const start = !outsideCurrentMonth && isStart(day);
    const end = !outsideCurrentMonth && isEnd(day);

    let extraClass = "";
    if (start) extraClass = "cal-day--start";
    else if (end) extraClass = "cal-day--end";
    else if (inRange) extraClass = "cal-day--in-range";

    return (
      <PickersDay
        {...rest}
        day={day}
        outsideCurrentMonth={outsideCurrentMonth}
        onClick={() => { if (!outsideCurrentMonth) handleDayClick(day); }}
        className={`${rest.className ?? ""} ${extraClass}`}
        selected={false}
        disableRipple
      />
    );
  }

  // +/- counter helper
  function changeCount(current: number, delta: number, min: number, max: number, onChange: (v: number) => void) {
    const next = current + delta;
    if (next < min || next > max) return;
    onChange(next);
  }

  const rightMonth = leftMonth.add(1, "month");

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
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

          {/* DATE TRIGGER */}
          <div
            className="searchbar-field searchbar-field--date-trigger"
            onClick={() => { setIsCalendarOpen(!isCalendarOpen); setIsGuestPanelOpen(false); }}
          >
            <span className="searchbar-icon">📅</span>
            <span className="searchbar-date-label-text">{buildDateLabel()}</span>
          </div>

          <div className="searchbar-divider" />

          {/* GUESTS TRIGGER */}
          <div
            className="searchbar-field searchbar-field--guests"
            onClick={() => { setIsGuestPanelOpen(!isGuestPanelOpen); setIsCalendarOpen(false); }}
          >
            <span className="searchbar-icon">👤</span>
            <span className="searchbar-guests-label">{buildGuestLabel()}</span>
            <span className="searchbar-chevron">{isGuestPanelOpen ? "▲" : "▼"}</span>
          </div>

          {/* SEARCH BUTTON */}
          <button className="searchbar-btn" onClick={props.onSearch} disabled={props.isLoading}>
            {props.isLoading ? "..." : "Search"}
          </button>
        </div>

        {/* ── Dual-Month Calendar Panel ── */}
        {isCalendarOpen && (
          <div className="calendar-panel">
            <div className="calendar-panel__hint">
              {selecting === "start" ? "Select check-in date" : "Select check-out date"}
            </div>
            <div className="calendar-panel__months">

              {/* Left month */}
              <DateCalendar
                value={checkIn}
                onChange={() => {}}
                referenceDate={leftMonth}
                onMonthChange={(m) => setLeftMonth(m.startOf("month"))}
                disablePast
                slots={{ day: CustomDay }}
                sx={calendarSx}
              />

              {/* Right month (always leftMonth + 1, navigation locked) */}
              <DateCalendar
                value={checkOut}
                onChange={() => {}}
                referenceDate={rightMonth}
                onMonthChange={(m) => setLeftMonth(m.subtract(1, "month").startOf("month"))}
                disablePast
                slots={{ day: CustomDay }}
                sx={calendarSx}
              />
            </div>
          </div>
        )}

        {/* ── Guests & Rooms Panel ── */}
        {isGuestPanelOpen && (
          <div className="guest-panel">
            <div className="guest-panel__header">Guests &amp; Rooms</div>

            <div className="guest-panel__row">
              <div><div className="guest-panel__row-title">Adults</div></div>
              <div className="guest-counter">
                <button className="guest-counter__btn" onClick={() => changeCount(props.searchForm.adults, -1, 1, 30, props.onAdultsChange)}>−</button>
                <span className="guest-counter__value">{props.searchForm.adults}</span>
                <button className="guest-counter__btn" onClick={() => changeCount(props.searchForm.adults, +1, 1, 30, props.onAdultsChange)}>+</button>
              </div>
            </div>

            <div className="guest-panel__row">
              <div>
                <div className="guest-panel__row-title">Children</div>
                <div className="guest-panel__row-sub">Ages 0 to 17</div>
              </div>
              <div className="guest-counter">
                <button className="guest-counter__btn" onClick={() => changeCount(props.searchForm.children, -1, 0, 10, props.onChildrenChange)}>−</button>
                <span className="guest-counter__value">{props.searchForm.children}</span>
                <button className="guest-counter__btn" onClick={() => changeCount(props.searchForm.children, +1, 0, 10, props.onChildrenChange)}>+</button>
              </div>
            </div>

            <div className="guest-panel__row">
              <div><div className="guest-panel__row-title">Rooms</div></div>
              <div className="guest-counter">
                <button className="guest-counter__btn" onClick={() => changeCount(props.searchForm.rooms, -1, 1, 9, props.onRoomsChange)}>−</button>
                <span className="guest-counter__value">{props.searchForm.rooms}</span>
                <button className="guest-counter__btn" onClick={() => changeCount(props.searchForm.rooms, +1, 1, 9, props.onRoomsChange)}>+</button>
              </div>
            </div>

            <button className="guest-panel__done" onClick={() => setIsGuestPanelOpen(false)}>Done</button>
          </div>
        )}

      </div>
    </LocalizationProvider>
  );
}

// MUI sx overrides shared by both calendars
const calendarSx = {
  fontFamily: "Roboto, sans-serif",
  width: 320,
  "& .MuiPickersCalendarHeader-label": {
    fontWeight: 700,
    fontSize: "16px",
  },
  "& .MuiDayCalendar-weekDayLabel": {
    color: "#64748B",
    fontWeight: 500,
  },
  // Start / end day — dark navy circle (like Hotels.com)
  "& .cal-day--start, & .cal-day--end": {
    backgroundColor: "#1E3A8A !important",
    color: "#fff !important",
    borderRadius: "50% !important",
    fontWeight: "700 !important",
  },
  // Days in range — light blue highlight
  "& .cal-day--in-range": {
    backgroundColor: "rgba(59,130,246,0.13) !important",
    borderRadius: "0 !important",
    color: "#1E3A8A !important",
  },
  // Remove default selected style so our custom overrides work
  "& .MuiPickersDay-root.Mui-selected": {
    backgroundColor: "transparent",
  },
};

export default SearchBar;
