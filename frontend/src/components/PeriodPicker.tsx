import { useMemo } from 'react';

export type PeriodSelection = {
  year: number;
  month: number;
};

type PeriodPickerProps = {
  value: PeriodSelection;
  onChange: (period: PeriodSelection) => void;
};

const monthNames = [
  'Styczeń',
  'Luty',
  'Marzec',
  'Kwiecień',
  'Maj',
  'Czerwiec',
  'Lipiec',
  'Sierpień',
  'Wrzesień',
  'Październik',
  'Listopad',
  'Grudzień',
];

export function getCurrentPeriod(): PeriodSelection {
  const today = new Date();
  return { year: today.getFullYear(), month: today.getMonth() + 1 };
}

export function PeriodPicker({ value, onChange }: PeriodPickerProps) {
  const yearOptions = useMemo(() => {
    const currentYear = new Date().getFullYear();
    return Array.from({ length: 11 }, (_, index) => currentYear - 5 + index);
  }, []);

  function shiftMonth(offset: number) {
    const date = new Date(value.year, value.month - 1 + offset, 1);
    onChange({ year: date.getFullYear(), month: date.getMonth() + 1 });
  }

  return (
    <div className="period-picker" aria-label="Wybrany miesiąc">
      <button
        aria-label="Poprzedni miesiąc"
        className="period-picker__arrow"
        onClick={() => shiftMonth(-1)}
        type="button"
      >
        ←
      </button>
      <select
        aria-label="Miesiąc"
        onChange={(event) => onChange({ ...value, month: Number(event.target.value) })}
        value={value.month}
      >
        {monthNames.map((name, index) => (
          <option key={name} value={index + 1}>
            {name}
          </option>
        ))}
      </select>
      <select
        aria-label="Rok"
        onChange={(event) => onChange({ ...value, year: Number(event.target.value) })}
        value={value.year}
      >
        {yearOptions.map((year) => (
          <option key={year} value={year}>
            {year}
          </option>
        ))}
      </select>
      <button
        aria-label="Następny miesiąc"
        className="period-picker__arrow"
        onClick={() => shiftMonth(1)}
        type="button"
      >
        →
      </button>
    </div>
  );
}

export function formatPeriod(period: PeriodSelection): string {
  return `${monthNames[period.month - 1]} ${period.year}`;
}

export function formatDateForPeriod(period: PeriodSelection, day = 1): string {
  return `${period.year}-${String(period.month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

export function getLastDayOfPeriod(period: PeriodSelection): string {
  const lastDay = new Date(period.year, period.month, 0).getDate();
  return formatDateForPeriod(period, lastDay);
}
