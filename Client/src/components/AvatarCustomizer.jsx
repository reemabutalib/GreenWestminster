import React, { useState, useEffect } from 'react';
import { useAuth } from './context/UserContext';
import '../styling/AvatarCustomizer.css'; 

const avatarStyles = [
  { name: 'Classic', img: '/avatars/classic.png', unlock: 0 },
  { name: 'Eco Hero', img: '/avatars/eco-hero.png', unlock: 100 },
  { name: 'Streak Master', img: '/avatars/streak-master.png', unlock: 7, unlockType: 'streak' },
];

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:80';

const AvatarCustomizer = () => {
  const { currentUser } = useAuth();
  const [userData, setUserData] = useState(null);

  useEffect(() => {
    const fetchUserData = async () => {
      if (!currentUser) return;
      const token = currentUser.token || localStorage.getItem('token');
      const userId = currentUser.id || currentUser.userId;
      const response = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (response.ok) {
        const data = await response.json();
        setUserData(data);
      }
    };
    fetchUserData();
  }, [currentUser]);

  const [selectedStyle, setSelectedStyle] = useState(avatarStyles[0].name);

  useEffect(() => {
    if (userData) {
      setSelectedStyle(userData.avatarStyle || avatarStyles[0].name);
    }
  }, [userData]);

  if (!userData) {
    return <div>Loading...</div>;
  }

  const canUnlock = (style) => {
    if (style.unlockType === 'streak') {
      return (userData.streak || 0) >= style.unlock;
    }
    return (userData.points || 0) >= style.unlock;
  };

  const handleStyleSelect = (style) => {
    if (canUnlock(style)) setSelectedStyle(style.name);
  };

  const handleSave = async () => {
    const res = await fetch(`${API_BASE_URL}/api/users/avatar`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        userId: userData.id,
        avatarStyle: selectedStyle,
        AvatarItems: [] // Send empty array since items are removed
      }),
    });
    if (res.ok) {
      alert('Avatar updated!');
    } else {
      alert('Failed to update avatar');
    }
  };

  return (
    <div className="avatar-customizer">
      <h2>Customise Your Avatar</h2>
      <p>
        Points: <strong>{userData.points ?? 0}</strong> | Streak: <strong>{userData.streak ?? 0}</strong>
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
      <button className="save-avatar-btn" onClick={handleSave}>
        Save Avatar
      </button>
    </div>
  );
};

export default AvatarCustomizer;