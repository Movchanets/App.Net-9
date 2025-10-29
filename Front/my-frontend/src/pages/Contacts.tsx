export default function Contacts() {
  return (
    <div className="grid gap-8 md:grid-cols-2">
      <div className="space-y-4">
        <h1 className="text-2xl font-semibold text-text">Контакти</h1>
        <p className="text-text-muted">Ми завжди на звʼязку. Напишіть нам!</p>

        <div className="space-y-2 text-sm text-text-muted">
          <p>E-mail: support@shopnine.com</p>
          <p>Телефон: +380 (00) 123-45-67</p>
          <p>Графік: Пн-Пт, 9:00 — 18:00</p>
        </div>
      </div>

      <form className="card space-y-4">
        <div>
          <label className="mb-1 block text-sm text-text-muted">Імʼя</label>
          <input className="w-full rounded-md border border-text/20 bg-transparent px-3 py-2 text-text outline-none focus:border-brand" />
        </div>
        <div>
          <label className="mb-1 block text-sm text-text-muted">E-mail</label>
          <input type="email" className="w-full rounded-md border border-text/20 bg-transparent px-3 py-2 text-text outline-none focus:border-brand" />
        </div>
        <div>
          <label className="mb-1 block text-sm text-text-muted">Повідомлення</label>
          <textarea rows={5} className="w-full rounded-md border border-text/20 bg-transparent px-3 py-2 text-text outline-none focus:border-brand" />
        </div>
        <button className="btn-primary w-full md:w-auto">Надіслати</button>
      </form>
    </div>
  )
}


