export default function Home() {
  return (
    <div className="space-y-10">
      <section className="grid gap-6 md:grid-cols-2 md:items-center">
        <div className="space-y-4">
          <h1 className="text-3xl font-bold tracking-tight text-text md:text-5xl">
            Магазин сучасних товарів
          </h1>
          <p className="max-w-prose text-text-muted">
            Відкрийте для себе добірку якісних товарів за чесними цінами. Швидка доставка,
            безпечна оплата і підтримка 24/7.
          </p>
          <div className="flex gap-3">
            <button className="btn-primary">До каталогу</button>
            <button className="rounded-md border border-text/20 px-4 py-2 text-text hover:bg-text/5">
              Дізнатись більше
            </button>
          </div>
        </div>
        <div className="card h-56 md:h-72" />
      </section>

      {/* Quick visual token check */}
      <section className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        <div className="h-12 rounded bg-surface-card/80 ring-1 ring-white/10" />
        <div className="h-12 rounded bg-surface ring-1 ring-white/10" />
        <div className="h-12 rounded bg-brand ring-1 ring-white/10" />
        <div className="h-12 rounded bg-brand-light ring-1 ring-white/10" />
      </section>

      <section>
        <h2 className="mb-4 text-xl font-semibold text-text">Популярні категорії</h2>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4].map((i) => (
            <div key={i} className="card h-40" />
          ))}
        </div>
      </section>

      <section>
        <h2 className="mb-4 text-xl font-semibold text-text">Рекомендації</h2>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4, 5, 6, 7, 8].map((i) => (
            <div key={i} className="card h-56" />
          ))}
        </div>
      </section>
    </div>
  )
}


