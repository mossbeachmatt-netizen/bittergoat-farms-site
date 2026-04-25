import "./BittergoatFarmsHomepage.css";

export default function BittergoatFarmsHomepage() {
  const featuredGames = [
    {
      title: "Boarding Pass",
      category: "Arcade",
      description: "Fast-paced airport boarding chaos.",
      badge: "Featured",
      image: "/images/games/boarding-pass.jpg",
    },
    {
      title: "Horse Racing",
      category: "Casino",
      description: "Aaand they're off! Bet on pixelated ponies.",
      badge: "New",
      image: "/images/games/horse-racing.jpg",
},
    {
      title: "Cocaine Bear",
      category: "Arcade",
      description: "Wild, chaotic gameplay.",
      badge: "New",
      image: "/images/games/cocaine-bear.jpg",
    },
    {
      title: "Golden Farms",
      category: "Casino",
      description: "Farm-themed slot machine fun.",
      badge: "Popular",
      image: "/images/games/golden-farms.jpg",
    },
  ];

  const allGames = [
    { title: "Boarding Pass", category: "Arcade" },
    { title: "Crab Hunt", category: "Arcade" },
    { title: "Cocaine Bear", category: "Arcade" },
    { title: "Mostly Peaceful", category: "Arcade" },
    { title: "Golden Farms", category: "Casino" },
    { title: "Goat Mines", category: "Casino" },
    { title: "Rocket", category: "Casino" },
    { title: "Horse Racing", category: "Casino" },
  ];

  const gameUrl = (title) => {
    switch (title) {
      case "Boarding Pass":
        return "/games/boarding-pass/index.html";
      case "Crab Hunt":
        return "/games/crab-hunt/index.html";
      case "Cocaine Bear":
        return "/games/cocaine-bear/index.html";
      case "Mostly Peaceful":
        return "/games/mostly-peaceful/index.html";
      case "Golden Farms":
        return "/games/golden-farms/index.html";
      case "Goat Mines":
        return "/games/goat-mines/index.html";
      case "Rocket":
        return "/games/rocket/index.html";
      case "Horse Racing":
        return "/games/horse-racing/index.html";
      default:
        return "#";
    }
  };

  return (
    <div className="bgf-page">
      <header className="bgf-header">
        <img src="/logo.png" alt="Bittergoat Farms" className="bgf-logo" />
        <nav className="bgf-nav">
          <a href="#home">Home</a>
          <a href="#games">All Games</a>
          <a href="#about">About</a>
        </nav>
      </header>

      <main id="home" className="bgf-main">
        <section className="bgf-section">
          <div className="bgf-container">
            <div className="card">
              <div className="pill">Indie arcade hub in progress</div>
              <h1>Homegrown HTML games with a little bite.</h1>
              <p>
                Bittergoat Farms is the home for fast, quirky game prototypes
                across arcade action and casino-style play.
              </p>
            </div>
          </div>
        </section>

        <section className="bgf-section">
          <div className="bgf-container">
            <h2>Featured Games</h2>
            <div className="bgf-card-grid">
              {featuredGames.map((game) => (
                <article key={game.title} className="card bgf-game-card">
                  <div className="bgf-game-meta">
                    <span className="badge badge-warm">{game.badge}</span>
                    <span className="badge badge-soft">{game.category}</span>
                  </div>

                  <div className="bgf-game-image-wrap">
                    <img
                      src={game.image}
                      alt={game.title}
                      className="bgf-game-image"
                    />
                  </div>

                  <h3>{game.title}</h3>
                  <p>{game.description}</p>

                  <button
                    className="btn btn-primary full"
                    onClick={() => {
                      window.location.href = gameUrl(game.title);
                    }}
                  >
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
              <h2>All Games</h2>

              <div className="bgf-library-grid">
                {allGames.map((game) => (
                  <div key={game.title} className="bgf-library-item">
                    <div>
                      <div className="bgf-library-title">{game.title}</div>
                      <div className="bgf-library-category">
                        {game.category}
                      </div>
                    </div>

                    <button
                      className="btn btn-primary"
                      onClick={() => {
                        window.location.href = gameUrl(game.title);
                      }}
                    >
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
                A growing collection of experimental HTML games across arcade
                and casino genres.
              </p>
            </div>
          </div>
        </section>
      </main>

      <footer className="bgf-footer">© Bittergoat Farms</footer>
    </div>
  );
}