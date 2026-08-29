// HomePage.tsx
// CONTAINER: manages search form state and calls the hotel search API.

import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import SearchBar from "../components/home/SearchBar";
import { getVietnamProvinces } from "../services/locationService";
import type { HotelSearchRequest, Province } from "../types/hotel.types";

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
  const navigate = useNavigate();
  const [searchForm, setSearchForm] = useState<HotelSearchRequest>({
    cityName: "",
    checkIn: getToday(),
    checkOut: getTomorrow(),
    adults: 1,
    children: 0,
    rooms: 1,
  });

  const [provinces, setProvinces] = useState<Province[]>([]);
  const [selectedProvince, setSelectedProvince] = useState<Province | null>(
    null,
  );
  const [provinceLoading, setProvinceLoading] = useState(true);
  const [provinceLoadError, setProvinceLoadError] = useState("");
  const [provinceRequest, setProvinceRequest] = useState(0);
  const [cityValidationError, setCityValidationError] = useState("");

  useEffect(() => {
    let ignore = false;
    getVietnamProvinces()
      .then((items) => {
        if (!ignore) setProvinces(items);
      })
      .catch((error: unknown) => {
        if (ignore) return;
        const fallback = "Failed to load destinations.";
        setProvinceLoadError(error instanceof Error ? error.message : fallback);
      })
      .finally(() => {
        if (!ignore) setProvinceLoading(false);
      });

    return () => {
      ignore = true;
    };
  }, [provinceRequest]);

  function handleProvinceRetry() {
    setProvinceLoading(true);
    setProvinceLoadError("");
    setProvinceRequest((value) => value + 1);
  }

  // Individual field updaters — called by SearchBar via props
  // Use the functional updater form so back-to-back calls in the same event (e.g. check-in + check-out) don't overwrite each other with stale state.
  function handleCityChange(value: string) {
    setSelectedProvince(null);
    setCityValidationError("");
    setSearchForm((prev) => ({ ...prev, cityName: value }));
  }
  function handleProvinceSelect(province: Province) {
    setSelectedProvince(province);
    setCityValidationError("");
    setSearchForm((prev) => ({ ...prev, cityName: province.name }));
  }
  function handleCheckInChange(value: string) {
    setSearchForm((prev) => ({ ...prev, checkIn: value }));
  }
  function handleCheckOutChange(value: string) {
    setSearchForm((prev) => ({ ...prev, checkOut: value }));
  }
  function handleAdultsChange(value: number) {
    setSearchForm((prev) => ({ ...prev, adults: value }));
  }
  function handleChildrenChange(value: number) {
    setSearchForm((prev) => ({ ...prev, children: value }));
  }
  function handleRoomsChange(value: number) {
    setSearchForm((prev) => ({ ...prev, rooms: value }));
  }

  // Called when the user clicks Search
  function handleSearch() {
    if (!selectedProvince || selectedProvince.name !== searchForm.cityName) {
      setCityValidationError("Please select a destination from the list.");
      return;
    }

    const query = new URLSearchParams({
      cityName: selectedProvince.name,
      checkIn: searchForm.checkIn,
      checkOut: searchForm.checkOut,
      adults: String(searchForm.adults),
      children: String(searchForm.children),
      rooms: String(searchForm.rooms),
    });
    navigate(`/search-results?${query.toString()}`);
  }

  return (
    <div className="home-page">
      {/* ── Hero Section ── */}
      <div className="hero-section">
        <div className="hero-overlay" />
        <div className="hero-content">
          <h1 className="hero-title">Your next trip starts here</h1>
          <p className="hero-subtitle">
            Discover amazing hotels at the best prices
          </p>
          <SearchBar
            searchForm={searchForm}
            onCityChange={handleCityChange}
            onCheckInChange={handleCheckInChange}
            onCheckOutChange={handleCheckOutChange}
            onAdultsChange={handleAdultsChange}
            onChildrenChange={handleChildrenChange}
            onRoomsChange={handleRoomsChange}
            onSearch={handleSearch}
            isLoading={false}
            provinces={provinces}
            selectedProvince={selectedProvince}
            onProvinceSelect={handleProvinceSelect}
            provinceLoading={provinceLoading}
            provinceLoadError={provinceLoadError || undefined}
            onProvinceRetry={handleProvinceRetry}
            cityValidationError={cityValidationError || undefined}
          />
        </div>
      </div>
    </div>
  );
}

export default HomePage;
