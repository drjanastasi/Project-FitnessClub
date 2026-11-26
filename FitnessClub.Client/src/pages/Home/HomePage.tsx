function HomePage() {
  return (
    <>
      <section className="welcome-section">
        <div className="welcome-text">
          <p className="eyebrow">FITNESSCLUB</p>
          <h1>Твій простір для тренувань</h1>
          <p>
            Обирай тренування, знаходь клуби та керуй
            своїми абонементами в одному місці.
          </p>

          <button className="primary-button">
            Переглянути тренування
          </button>
        </div>

        <div className="welcome-decoration">
          <div className="decoration-circle">
            <span>FC</span>
          </div>
        </div>
      </section>

      <section className="stats-section">
        <div className="stat-card">
          <div className="stat-icon blue">◈</div>
          <div>
            <p className="stat-label">Тренування</p>
            <h2>12+</h2>
            <p className="stat-description">Доступних програм</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon purple">⌖</div>
          <div>
            <p className="stat-label">Клуби</p>
            <h2>8+</h2>
            <p className="stat-description">Місць для тренувань</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon green">▣</div>
          <div>
            <p className="stat-label">Абонементи</p>
            <h2>3</h2>
            <p className="stat-description">Варіанти доступу</p>
          </div>
        </div>
      </section>

      <section className="workouts-section">
        <div className="section-heading">
          <div>
            <p className="eyebrow">ТВОЯ АКТИВНІСТЬ</p>
            <h2>Популярні тренування</h2>
          </div>

          <button className="secondary-button">
            Усі тренування →
          </button>
        </div>

        <div className="workout-grid">
          <article className="workout-card">
            <div className="workout-image workout-one">
              <span className="workout-tag">Силове</span>
            </div>
            <div className="workout-info">
              <h3>Силові тренування</h3>
              <p>Тренування з обладнанням</p>
              <button className="card-button">Детальніше</button>
            </div>
          </article>

          <article className="workout-card">
            <div className="workout-image workout-two">
              <span className="workout-tag">Кардіо</span>
            </div>
            <div className="workout-info">
              <h3>Кардіотренування</h3>
              <p>Активні тренування</p>
              <button className="card-button">Детальніше</button>
            </div>
          </article>

          <article className="workout-card">
            <div className="workout-image workout-three">
              <span className="workout-tag">Групове</span>
            </div>
            <div className="workout-info">
              <h3>Групові заняття</h3>
              <p>Заняття з тренером</p>
              <button className="card-button">Детальніше</button>
            </div>
          </article>
        </div>
      </section>

      <footer className="footer">
        <p>© 2026 FitnessClub. Всі права захищені.</p>
      </footer>
    </>
  )
}

export default HomePage