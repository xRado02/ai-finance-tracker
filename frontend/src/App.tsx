import './App.css';

export default function App() {
  return (
    <main className="app-shell">
      <section className="workspace">
        <header className="workspace__header">
          <div>
            <p className="workspace__eyebrow">Local finance</p>
            <h1>AI Finance Tracker</h1>
          </div>
          <span className="workspace__status">Frontend scaffold</span>
        </header>

        <div className="workspace__grid">
          <section className="panel">
            <h2>Add transaction</h2>
            <p>Transaction form will connect to the existing finance API in the next phases.</p>
          </section>

          <section className="panel">
            <h2>Transaction history</h2>
            <p>History will load from <code>/api/transactions</code>.</p>
          </section>
        </div>
      </section>
    </main>
  );
}
