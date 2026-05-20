interface BarChartItem {
  label: string
  value: number
}

interface BarChartProps {
  title: string
  items: BarChartItem[]
  valueFormatter?: (value: number) => string
  emptyMessage?: string
}

export function BarChart({ title, items, valueFormatter, emptyMessage }: BarChartProps) {
  const max = items.length > 0 ? Math.max(...items.map((i) => i.value), 1) : 1
  const format = valueFormatter ?? ((v: number) => String(v))

  return (
    <div className="chart-panel">
      <h3 className="chart-title">{title}</h3>
      {items.length === 0 ? (
        <p className="status-message">{emptyMessage ?? 'No data yet.'}</p>
      ) : (
        <ul className="bar-chart" role="list">
          {items.map((item) => (
            <li key={item.label} className="bar-chart-row">
              <span className="bar-chart-label" title={item.label}>
                {item.label}
              </span>
              <div className="bar-chart-track" aria-hidden>
                <div
                  className="bar-chart-fill"
                  style={{ width: `${Math.max((item.value / max) * 100, 4)}%` }}
                />
              </div>
              <span className="bar-chart-value">{format(item.value)}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
