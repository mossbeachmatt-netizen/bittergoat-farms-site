import "./BittergoatFarmsHomepage.css";

export default function BittergoatFarmsHomepage() {
  const featuredGames = [
    { title: "Boarding Pass", category: "Arcade", description: "Fast-paced airport boarding chaos with timing and strategy.", badge: "Featured", icon: "🕹️" },
    { title: "Golden Farms", category: "Casino", description: "Spin the reels and harvest big wins in this farm-themed slot experience.", badge: "Popular", icon: "🎰" },
    { title: "Crab Hunt", category: "Arcade", description: "Quick reflex arcade action chasing crabs and high scores.", badge: "Hot", icon: "🕹️" },
    { title: "Cocaine Bear", category: "Arcade", description: "Wild, chaotic gameplay inspired by an unstoppable force of nature.", badge: "New", icon: "🕹️" },
  ];

  const allGames = [
    { title: "Boarding Pass", category: "Arcade" },
    { title: "Crab Hunt", category: "Arcade" },
    { title: "Cocaine Bear", category: "Arcade" },
    { title: "Mostly Peaceful", category: "Arcade" },
    { title: "Golden Farms", category: "Casino" },
  ];

  const gameUrl = (title) => {
    switch (title) {
      case "Boarding Pass":
        return "/games/boarding-pass/index.html";
      case "Golden Farms":
        return "/games/golden-farms/index.html";
      case "Crab Hunt":
        return "/games/crab-hunt/index.html";
      case "Cocaine Bear":
        return "/games/cocaine-bear/index.html";
      default:
        return "/games/mostly-peaceful/index.html";
    }
  };

  return (
    <div className="bgf-page">
      <div className="bgf-grid-overlay" />

      <header className="bgf-header">
        <div className="bgf-container bgf-header-inner">
          <img src="/logo.png" alt="Bittergoat Farms Logo" className="bgf-logo" />
          <nav className="bgf-nav">
            <a href="#home">Home</a>
            <a href="#categories">Categories</a>
            <a href="#games">All Games</a>
            <a href="#about">About</a>
          </nav>
        </div>
      </header>

      <main id="home" className="bgf-main">
        <section className="bgf-hero-section">
          <div className="bgf-container bgf-hero-grid">
            <div className="bgf-hero-copy card card-glass">
              <div className="pill">Indie arcade hub in progress</div>
              <h1>Homegrown HTML games with a little bite.</h1>
              <p>
                Bittergoat Farms is the home for fast, quirky game prototypes across two lanes:
                arcade action and casino-style play. Browse the library, launch a favorite,
                and check back as new games drop.
              </p>
              <div className="bgf-hero-actions">
                <a href="#categories" className="btn btn-primary">Explore Games</a>
                <a href="#games" className="btn btn-accent">Jump to Library</a>
              </div>
            </div>

            <div className="bgf-status card card-dark" id="categories">
              <div className="bgf-status-top">
                <div>
                  <div className="eyebrow light">Studio Hub</div>
                  <h2>Bittergoat Farms</h2>
                </div>
                <div className="badge-light">5 Games Live</div>
              </div>

              <div className="bgf-category-list">
                <div className="bgf-category-card">
                  <div className="bgf-category-icon">🕹️</div>
                  <h3>Arcade</h3>
                  <p>Quick-session games built for reflexes, chaos, and replayability.</p>
                </div>
                <div className="bgf-category-card">
                  <div className="bgf-category-icon">🎰</div>
                  <h3>Casino</h3>
                  <p>Social casino experiments with satisfying feedback and playful themes.</p>
                </div>
              </div>
            </div>
          </div>
        </section>

        <section className="bgf-section">
          <div className="bgf-container">
            <div className="bgf-section-head">
              <div>
                <div className="eyebrow">Featured Games</div>
                <h2>Start with the favorites</h2>
              </div>
            </div>

            <div className="bgf-card-grid">
              {featuredGames.map((game) => (
                <article key={game.title} className="card bgf-game-card">
                  <div className="bgf-game-meta">
                    <span className="badge badge-warm">{game.badge}</span>
                    <span className="badge badge-soft">{game.category}</span>
                  </div>
                  <div className="bgf-game-icon">{game.icon}</div>
                  <h3>{game.title}</h3>
                  <p>{game.description}</p>
                  <button className="btn btn-primary full" onClick={() => window.location.href = gameUrl(game.title)}>
                    Play Now
                  </button>
                </article>
              ))}
            </div>
          </div>
        </section>

        <section id="games" className="bgf-section">
          <div className="bgf-container">
            <div className="card bgf-library-card">
              <div className="bgf-section-head">
                <div>
                  <div className="eyebrow">Game Library</div>
                  <h2>All Games</h2>
                </div>
                <p className="bgf-muted">Arcade and casino prototypes, all in one place.</p>
              </div>

              <div className="bgf-library-grid">
                {allGames.map((game) => (
                  <div key={game.title} className="bgf-library-item">
                    <div>
                      <div className="bgf-library-title">{game.title}</div>
                      <div className="bgf-library-category">{game.category}</div>
                    </div>
                    <button className="btn btn-primary" onClick={() => window.location.href = gameUrl(game.title)}>
                      Play
                    </button>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </section>

        <section id="about" className="bgf-section bgf-section-bottom">
          <div className="bgf-container">
            <div className="card card-dark bgf-about-card">
              <h2>About Bittergoat Farms</h2>
              <p>
                A growing collection of experimental HTML games across arcade and casino genres.
                Built for fun, speed, personality, and constant iteration.
              </p>
            </div>
          </div>
        </section>
      </main>

      <footer className="bgf-footer">© Bittergoat Farms</footer>
    </div>
  );
}