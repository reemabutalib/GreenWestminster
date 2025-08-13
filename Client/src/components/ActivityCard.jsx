import '../styling/ActivityCard.css';

const ActivityCard = ({ activity, onCompleteClick, isPending }) => {
  // Helper: truthy variations
  const isTrueProperty = (obj, propNames) => {
    if (!obj) return false;
    for (const propName of propNames) {
      const v = obj[propName];
      if (v === true) return true;
      if (v === 1) return true;
      if (typeof v === 'string' && v.toLowerCase() === 'true') return true;
    }
    return false;
  };

  // Frequency
  const isDaily = isTrueProperty(activity, ['isDaily', 'IsDaily', 'isdaily']);
  const isWeeklyExplicit = isTrueProperty(activity, ['isWeekly', 'IsWeekly', 'isweekly', 'weekly']);
  const isWeekly = isWeeklyExplicit || (!isDaily && activity.isDaily === false);

  // Heuristic: does this activity scale with quantity?
  const isQuantityBased = (() => {
    const cat = String(activity.category || '').toLowerCase();
    if (['waste', 'waste reduction', 'water', 'transport', 'energy', 'food'].some(c => cat.includes(c))) {
      // likely quantity-based in these categories, but still scan text for hints
    }
    const text = `${activity.title || ''} ${activity.description || ''}`.toLowerCase();

    // keywords that imply variable quantity (distance, time, count, volume, weight)
    const hints = [
      'per ', 'each ', 'km', 'mile', 'minutes', 'hours', 'bags', 'bottles', 'items', 'kg', 'kilogram', 'g ', 'grams',
      'l ', 'litre', 'liter', 'kwh', 'loads', 'steps', 'bins', 'cups', 'sheets'
    ];
    if (hints.some(h => text.includes(h))) return true;

    // explicit flags if your API ever sends them
    if (isTrueProperty(activity, ['isQuantityBased', 'quantityBased'])) return true;

    return ['waste', 'water', 'transport'].some(c => cat.includes(c)); // gentle default
  })();

  const handleCompleteActivity = () => {
    if (!isPending) onCompleteClick(activity);
  };

  const pointsVal = Number(activity.pointsValue ?? 0);
  const pointsLabel = isQuantityBased ? `~+${pointsVal}` : `+${pointsVal}`;

  return (
    <div className={`activity-card ${isPending ? 'completed' : ''}`}>
      {/* Category Tag */}
      <div className="card-tag">{activity.category}</div>

      {/* Pending Review badge */}
      {isPending && <div className="pending-status">⏳ Pending Review</div>}

      {/* Title & Description */}
      <h3>{activity.title}</h3>
      <p>{activity.description}</p>

      {/* Metadata: points & frequency */}
      <div className="activity-meta">
        <div className={`points ${isQuantityBased ? 'points-estimated' : ''}`}>
          <span className="points-value">{pointsLabel}</span> points
        </div>
        <div className="frequency">
          {isDaily && <span className="badge daily">Daily</span>}
          {isWeekly && <span className="badge weekly">Weekly</span>}
        </div>
      </div>

      {/* Guidance for quantity-based activities */}
      {isQuantityBased && (
        <div className="est-note" title="Your final points depend on the amount you did; an admin may adjust after review.">
          <span className="est-icon" aria-hidden>ℹ️</span>
          Points shown are an estimate. Final points depend on the quantity you did (e.g., distance, time, items).
        </div>
      )}

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
