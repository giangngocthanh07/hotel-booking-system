import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { searchHotels } from "../services/hotelService";
import type {
  SearchCriteria,
  SearchHotelResult,
} from "../types/hotel.types";
import { parseSearchCriteria } from "../utils/searchCriteria";
import "./SearchResultsPage.css";

const FALLBACK_IMAGE = "/hotel-placeholder.svg";

function formatPrice(value: number): string {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(value);
}

function CriteriaSummary({ criteria }: { criteria: SearchCriteria }) {
  return (
    <div className="results-summary" aria-label="Điều kiện tìm kiếm">
      <strong>📍 {criteria.cityName}</strong>
      <span>📅 {criteria.checkIn} → {criteria.checkOut}</span>
      <span>👤 {criteria.adults} người lớn</span>
      <span>🧒 {criteria.children} trẻ em</span>
      <span>🛏️ {criteria.rooms} phòng</span>
    </div>
  );
}

function HotelCard({ hotel }: { hotel: SearchHotelResult }) {
  return (
    <article className="hotel-result-card">
      <img
        className="hotel-result-card__image"
        src={hotel.coverImageUrl || FALLBACK_IMAGE}
        alt={`Ảnh ${hotel.name}`}
        onError={(event) => {
          event.currentTarget.onerror = null;
          event.currentTarget.src = FALLBACK_IMAGE;
        }}
      />
      <div className="hotel-result-card__body">
        <div className="hotel-result-card__heading">
          <div>
            <h2>{hotel.name}</h2>
            <p className="hotel-result-card__location">
              📍 {hotel.address}, {hotel.cityName}, {hotel.countryName}
            </p>
          </div>
          <div className="hotel-result-card__rating">
            <strong>{hotel.avgRating.toFixed(1)}</strong>
            <span>{hotel.reviewCount} đánh giá</span>
          </div>
        </div>
        {hotel.description && (
          <p className="hotel-result-card__description">{hotel.description}</p>
        )}
        <div className="hotel-result-card__footer">
          <div className="hotel-result-card__capacity">
            Tối đa {hotel.maxAdultCapacity} người lớn, {hotel.maxChildCapacity} trẻ em
          </div>
          <div className="hotel-result-card__price">
            <span>Giá từ</span>
            <strong>{formatPrice(hotel.priceFrom)}</strong>
            <small>Còn {hotel.availableRooms} phòng</small>
          </div>
        </div>
      </div>
    </article>
  );
}

function SearchResultsContent({ criteria }: { criteria: SearchCriteria }) {
  const [hotels, setHotels] = useState<SearchHotelResult[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [retryCount, setRetryCount] = useState(0);

  useEffect(() => {
    let ignore = false;
    searchHotels(criteria)
      .then((results) => {
        if (!ignore) setHotels(results);
      })
      .catch((reason: unknown) => {
        if (ignore) return;
        const fallback = "Không thể tìm kiếm khách sạn.";
        setError(reason instanceof Error ? reason.message : fallback);
      })
      .finally(() => {
        if (!ignore) setIsLoading(false);
      });

    return () => {
      ignore = true;
    };
  }, [criteria, retryCount]);

  function retrySearch() {
    setIsLoading(true);
    setError("");
    setRetryCount((value) => value + 1);
  }

  return (
    <main className="results-page">
      <div className="results-heading">
        <div>
          <p className="results-eyebrow">Kết quả tìm kiếm</p>
          <h1>Khách sạn tại {criteria.cityName}</h1>
        </div>
        <Link className="results-edit-link" to="/">Thay đổi tìm kiếm</Link>
      </div>
      <CriteriaSummary criteria={criteria} />

      {isLoading && <div className="results-state" role="status">Đang tìm khách sạn phù hợp...</div>}
      {!isLoading && error && (
        <div className="results-state results-state--error" role="alert">
          <h2>Không thể tải kết quả</h2>
          <p>{error}</p>
          <button className="results-action" onClick={retrySearch}>
            Thử lại
          </button>
        </div>
      )}
      {!isLoading && !error && hotels.length === 0 && (
        <div className="results-state">
          <h2>Không tìm thấy khách sạn phù hợp.</h2>
          <p>Hãy thử một ngày lưu trú hoặc số lượng phòng khác.</p>
        </div>
      )}
      {!isLoading && !error && hotels.length > 0 && (
        <div className="hotel-results-list">
          {hotels.map((hotel) => <HotelCard hotel={hotel} key={hotel.id} />)}
        </div>
      )}
    </main>
  );
}

function SearchResultsPage() {
  const [searchParams] = useSearchParams();
  const query = searchParams.toString();
  const criteria = useMemo(
    () => parseSearchCriteria(new URLSearchParams(query)),
    [query],
  );

  if (!criteria) {
    return (
      <main className="results-page results-state">
        <h1>Điều kiện tìm kiếm không hợp lệ.</h1>
        <p>Vui lòng quay lại và chọn đầy đủ tỉnh thành, ngày và số khách.</p>
        <Link className="results-action" to="/">Quay lại tìm kiếm</Link>
      </main>
    );
  }

  return <SearchResultsContent criteria={criteria} key={query} />;
}

export default SearchResultsPage;
