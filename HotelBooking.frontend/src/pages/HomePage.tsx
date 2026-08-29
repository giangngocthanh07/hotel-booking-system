// HomePage.tsx
// CONTAINER: manages search form state and calls the hotel search API.

import { useState } from "react";
import SearchBar from "../components/home/SearchBar";
import type { HotelSearchRequest } from "../types/hotel.types";

// Helper: returns today date as "YYYY-MM-DD"
function getToday(): string {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, "0");
  const day = String(now.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

// Helper: returns tomorrow date as "YYYY-MM-DD"
function getTomorrow(): string {
  const now = new Date();
  now.setDate(now.getDate() + 1);
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, "0");
  const day = String(now.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function HomePage() {
  const [searchForm, setSearchForm] = useState<HotelSearchRequest>({
    cityName: "",
    checkIn: getToday(),
    checkOut: getTomorrow(),
    adults: 1,
    children: 0,
    rooms: 1,
  });

  const [isLoading, setIsLoading] = useState(false);

  // Individual field updaters — called by SearchBar via props
  function handleCityChange(value: string) {
    setSearchForm({ ...searchForm, cityName: value });
  }
  function handleCheckInChange(value: string) {
    setSearchForm({ ...searchForm, checkIn: value });
  }
  function handleCheckOutChange(value: string) {
    setSearchForm({ ...searchForm, checkOut: value });
  }
  function handleAdultsChange(value: number) {
    setSearchForm({ ...searchForm, adults: value });
  }
  function handleChildrenChange(value: number) {
    setSearchForm({ ...searchForm, children: value });
  }
  function handleRoomsChange(value: number) {
    setSearchForm({ ...searchForm, rooms: value });
  }

  // Called when the user clicks Search
  async function handleSearch() {
    setIsLoading(true);
    // TODO: call hotelService.searchHotels(searchForm) and navigate to results
    // For now just log to console so we can verify it works
    console.log("Searching with:", searchForm);
    setTimeout(() => setIsLoading(false), 800);
  }

  return (
    <div className="home-page">
      {/* ── Hero Section ── */}
      <div className="hero-section">
        <div className="hero-overlay" />
        <div className="hero-content">
          <h1 className="hero-title">Your next trip starts here</h1>
          <p className="hero-subtitle">Discover amazing hotels at the best prices</p>
          <SearchBar
            searchForm={searchForm}
            onCityChange={handleCityChange}
            onCheckInChange={handleCheckInChange}
            onCheckOutChange={handleCheckOutChange}
            onAdultsChange={handleAdultsChange}
            onChildrenChange={handleChildrenChange}
            onRoomsChange={handleRoomsChange}
            onSearch={handleSearch}
            isLoading={isLoading}
          />
        </div>
      </div>
    </div>
  );
}

export default HomePage;
