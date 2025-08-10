import '../styling/ActivityCard.css';

const ActivityCard = ({ activity, onCompleteClick, isPending }) => {
  // Helper function to check boolean properties safely with multiple variations
  const isTrueProperty = (obj, propNames) => {
    if (!obj) return false;

    for (const propName of propNames) {
      if (obj[propName] === true) return true;
      if (typeof obj[propName] === 'string' && obj[propName].toLowerCase() === 'true') return true;
      if (obj[propName] === 1) return true;
    }
    return false;
  };

  // Determine frequency
  const isDaily = isTrueProperty(activity, ['isDaily', 'IsDaily', 'isdaily']);
  const isWeeklyExplicit = isTrueProperty(activity, ['isWeekly', 'IsWeekly', 'isweekly', 'weekly']);
  const isWeekly = isWeeklyExplicit || (!isDaily && activity.isDaily === false);

  // Click handler
  const handleCompleteActivity = () => {
    if (!isPending) {
      onCompleteClick(activity);
    }
  };

  return (
    <div className={`activity-card ${isPending ? 'completed' : ''}`}>
      {/* Category Tag */}
      <div className="card-tag">{activity.category}</div>

      {/* Pending Review badge */}
      {isPending && (
        <div className="pending-status">⏳ Pending Review</div>
      )}

      {/* Title & Description */}
      <h3>{activity.title}</h3>
      <p>{activity.description}</p>

      {/* Metadata: points & frequency badges */}
      <div className="activity-meta">
        <div className="points">
          <span className="points-value">+{activity.pointsValue}</span> points
        </div>
        <div className="frequency">
          {isDaily && <span className="badge daily">Daily</span>}
          {isWeekly && <span className="badge weekly">Weekly</span>}
        </div>
      </div>

      {/* Button */}
      <button
        className="complete-btn"
        onClick={handleCompleteActivity}
        disabled={isPending}
      >
        {isPending ? 'Pending Review' : 'Complete Activity'}
      </button>
    </div>
  );
};

export default ActivityCard;
