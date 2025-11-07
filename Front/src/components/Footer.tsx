export function Footer() {
  return (
    <footer className="mt-16 border-t border-text/10 bg-surface-card/95">
      <div className="mx-auto w-full max-w-7xl px-4 py-8 text-sm text-text-muted">
        <div className="flex flex-col items-center justify-between gap-4 md:flex-row">
          <p>© {new Date().getFullYear()} ShopNine. Всі права захищено.</p>
          <div className="flex items-center gap-4">
            <a href="#" className="hover:text-text">Умови</a>
            <a href="#" className="hover:text-text">Конфіденційність</a>
            <a href="#" className="hover:text-text">Підтримка</a>
          </div>
        </div>
      </div>
    </footer>
  )
}


