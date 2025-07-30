import React, { useState } from 'react';
import { useAuth } from './context/UserContext';
import '../styling/AvatarCustomizer.css'; 

const avatarStyles = [
  { name: 'Classic', img: '/avatars/classic.png', unlock: 0 },
  { name: 'Eco Hero', img: '/avatars/eco-hero.png', unlock: 100 },
  { name: 'Streak Master', img: '/avatars/streak-master.png', unlock: 7, unlockType: 'streak' },
  // Add more styles as needed
];

const visualItems = [
  { name: 'Leaf Hat', img: '/avatars/items/leaf-hat.png', unlock: 50 },
  { name: 'Recycling Cape', img: '/avatars/items/cape.png', unlock: 200 },
  { name: 'Golden Badge', img: '/avatars/items/gold-badge.png', unlock: 14, unlockType: 'streak' },
  // Add more items as needed
];

const AvatarCustomizer = () => {
  const { currentUser, updateAvatar } = useAuth();
  // Example: currentUser.points, currentUser.streak
  const [selectedStyle, setSelectedStyle] = useState(currentUser.avatarStyle || avatarStyles[0].name);
  const [selectedItems, setSelectedItems] = useState(currentUser.avatarItems || []);

  const canUnlock = (item) => {
    if (item.unlockType === 'streak') {
      return (currentUser.streak || 0) >= item.unlock;
    }
    return (currentUser.points || 0) >= item.unlock;
  };

  const handleStyleSelect = (style) => {
    if (canUnlock(style)) setSelectedStyle(style.name);
  };

  const handleItemToggle = (item) => {
    if (!canUnlock(item)) return;
    setSelectedItems((prev) =>
      prev.includes(item.name)
        ? prev.filter((i) => i !== item.name)
        : [...prev, item.name]
    );
  };

  const handleSave = () => {
    // Save avatar choices to backend or context
    updateAvatar({ avatarStyle: selectedStyle, avatarItems: selectedItems });
    alert('Avatar updated!');
  };

  return (
    <div className="avatar-customizer">
      <h2>Customize Your Avatar</h2>
      <p>
        Points: <strong>{currentUser.points}</strong> | Streak: <strong>{currentUser.streak}</strong>
      </p>
      <div className="avatar-section">
        <h3>Choose Your Style</h3>
        <div className="avatar-style-list">
          {avatarStyles.map((style) => (
            <button
              key={style.name}
              className={`avatar-style-btn${selectedStyle === style.name ? ' selected' : ''}`}
              onClick={() => handleStyleSelect(style)}
              disabled={!canUnlock(style)}
              title={
                canUnlock(style)
                  ? `Select ${style.name}`
                  : style.unlockType === 'streak'
                  ? `Unlock with ${style.unlock} day streak`
                  : `Unlock at ${style.unlock} points`
              }
            >
              <img src={style.img} alt={style.name} className="avatar-img" />
              <div>{style.name}</div>
              {!canUnlock(style) && (
                <div className="locked-overlay">🔒</div>
              )}
            </button>
          ))}
        </div>
      </div>
      <div className="avatar-section">
        <h3>Unlockable Items</h3>
        <div className="avatar-items-list">
          {visualItems.map((item) => (
            <button
              key={item.name}
              className={`avatar-item-btn${selectedItems.includes(item.name) ? ' selected' : ''}`}
              onClick={() => handleItemToggle(item)}
              disabled={!canUnlock(item)}
              title={
                canUnlock(item)
                  ? `Toggle ${item.name}`
                  : item.unlockType === 'streak'
                  ? `Unlock with ${item.unlock} day streak`
                  : `Unlock at ${item.unlock} points`
              }
            >
              <img src={item.img} alt={item.name} className="avatar-img" />
              <div>{item.name}</div>
              {!canUnlock(item) && (
                <div className="locked-overlay">🔒</div>
              )}
            </button>
          ))}
        </div>
      </div>
      <button className="save-avatar-btn" onClick={handleSave}>
        Save Avatar
      </button>
    </div>
  );
};

export default AvatarCustomizer;