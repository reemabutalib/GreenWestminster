import '../styling/ActivityCard.css';

const ActivityCard = ({ activity, onCompleteClick }) => {
  // Helper function to check boolean properties safely with multiple variations
  const isTrueProperty = (obj, propNames) => {
    if (!obj) return false;
    
    for (const propName of propNames) {
      // Check property directly
      if (obj[propName] === true) return true;
      
      // Check string representation "true"
      if (typeof obj[propName] === 'string' && obj[propName].toLowerCase() === 'true') return true;
      
      // Check numeric representation 1
      if (obj[propName] === 1) return true;
    }
    return false;
  };
  
  // Determine activity frequency types using the isTrueProperty helper
  const isDaily = isTrueProperty(activity, ['isDaily', 'IsDaily', 'isdaily']);
  const isWeeklyExplicit = isTrueProperty(activity, ['isWeekly', 'IsWeekly', 'isweekly', 'weekly']);
  // For weekly: either explicit isWeekly=true OR isDaily=false
  const isWeekly = isWeeklyExplicit || (!isDaily && activity.isDaily === false);

  const handleCompleteActivity = () => {
    onCompleteClick(activity);
  };

  return (
    <div className="activity-card">
      <div className="card-tag">{activity.category}</div>
      
      <h3>{activity.title}</h3>
      <p>{activity.description}</p>
      
      <div className="activity-meta">
        <div className="points">
          <span className="points-value">+{activity.pointsValue}</span> points
        </div>
        
        <div className="frequency">
          {isDaily && <span className="badge daily">Daily</span>}
          {isWeekly && <span className="badge weekly">Weekly</span>}
        </div>
      </div>
      
      <button 
        className="complete-btn"
        onClick={handleCompleteActivity}
      >
        Complete Activity
      </button>
    </div>
  );
};

export default ActivityCard;