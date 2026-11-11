export function ownerPageIndex(value: string | null): number {
  const number = Number(value);
  return Number.isSafeInteger(number) && number > 0 && number <= 2147483647 ? number : 1;
}

export function ownerHotelDate(value?: string | null): string {
  const date = value ? new Date(value) : null;
  return date && !Number.isNaN(date.getTime()) ? date.toLocaleDateString("en-GB") : "Not provided";
}
