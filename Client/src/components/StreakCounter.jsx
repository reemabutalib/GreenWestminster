import '../styling/StreakCounter.css';

const StreakCounter = ({ streak }) => {
  return (
    <div className="streak-counter">
      <span className="streak-value">{streak}</span>
      <span className="streak-label">
        Day{streak !== 1 ? 's' : ''} Streak
        <span className="streak-flame">🔥</span>
      </span>
    </div>
  );
};

export default StreakCounter;