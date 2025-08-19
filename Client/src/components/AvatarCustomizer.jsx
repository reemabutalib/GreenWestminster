// AvatarCustomizer.jsx
import React, { useState, useEffect, useMemo } from 'react';
import { useAuth } from './context/UserContext';
import '../styling/AvatarCustomizer.css';

const API_BASE_URL = (
  import.meta.env.DEV
    ? '' 
    : (import.meta.env.VITE_API_URL || 'https://greenwestminster.onrender.com')
).replace(/\/$/, '');

// Level thresholds you already use in the app
const LEVEL_THRESHOLDS = {
  Bronze: 0,
  Silver: 500,
  Gold: 1000,
  Platinum: 5000,
};

// Build the full avatar catalog (3 per tier)
function buildAvatarStyles() {
  return [
    // Bronze (0+)
    { name: 'Sprout Seeker',     tier: 'Bronze',   img: '/avatars/bronze-sprout-seeker.png',     unlock: LEVEL_THRESHOLDS.Bronze },
    { name: 'Seedling Adventurer', tier: 'Bronze', img: '/avatars/bronze-seedling-adventurer.png', unlock: LEVEL_THRESHOLDS.Bronze },
    { name: 'Eco Explorer',       tier: 'Bronze',   img: '/avatars/bronze-eco-explorer.png',       unlock: LEVEL_THRESHOLDS.Bronze },

    // Silver (500+)
    { name: 'Leaf Guardian',     tier: 'Silver',   img: '/avatars/silver-leaf-guardian.png',     unlock: LEVEL_THRESHOLDS.Silver },
    { name: 'Eco Pathfinder',    tier: 'Silver',   img: '/avatars/silver-eco-pathfinder.png',    unlock: LEVEL_THRESHOLDS.Silver },
    { name: 'River Protector',   tier: 'Silver',   img: '/avatars/silver-river-protector.png',   unlock: LEVEL_THRESHOLDS.Silver },

    // Gold (1000+)
    { name: 'Sunlight Steward',  tier: 'Gold',     img: '/avatars/gold-sunlight-steward.png',    unlock: LEVEL_THRESHOLDS.Gold },
    { name: 'Forest Champion',   tier: 'Gold',     img: '/avatars/gold-forest-champion.png',     unlock: LEVEL_THRESHOLDS.Gold },
    { name: 'Wildlife Guardian', tier: 'Gold',     img: '/avatars/gold-wildlife-guardian.png',   unlock: LEVEL_THRESHOLDS.Gold },

    // Platinum (5000+)
    { name: 'Planet Protector',  tier: 'Platinum', img: '/avatars/platinum-planet-protector.png', unlock: LEVEL_THRESHOLDS.Platinum },
    { name: 'Harmony Keeper',    tier: 'Platinum', img: '/avatars/platinum-harmony-keeper.png',   unlock: LEVEL_THRESHOLDS.Platinum },
    { name: 'Gaia’s Guardian',   tier: 'Platinum', img: '/avatars/platinum-gaias-guardian.png',   unlock: LEVEL_THRESHOLDS.Platinum },
  ];
}

const AvatarCustomizer = () => {
  const { currentUser } = useAuth();
  const [userData, setUserData] = useState(null);
  const [selectedStyle, setSelectedStyle] = useState(null);

  const avatarStyles = useMemo(buildAvatarStyles, []);
  const totalCount = avatarStyles.length;

  useEffect(() => {
    const fetchUserData = async () => {
      if (!currentUser) return;
      const token = currentUser.token || localStorage.getItem('token');
      const userId = currentUser.id || currentUser.userId;
      const response = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (response.ok) {
        const data = await response.json();
        setUserData(data);
        // default to previously chosen avatar, or the first Bronze
        const fallback = avatarStyles.find(a => a.tier === 'Bronze')?.name;
        setSelectedStyle(data.avatarStyle || fallback);
      }
    };
    fetchUserData();
  }, [currentUser, avatarStyles]);

  if (!userData) return <div>Loading...</div>;

  const canUnlock = (style) => (userData.points || 0) >= style.unlock;

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
        AvatarItems: [], // items removed
      }),
    });
    alert(res.ok ? 'Avatar updated!' : 'Failed to update avatar');
  };

  // Nice little unlocked counter
  const unlockedCount = avatarStyles.filter(canUnlock).length;

  return (
    <div className="avatar-customizer">
      <h2>Customise Your Avatar</h2>
      <p>
        Points: <strong>{userData.points ?? 0}</strong> •
        <span style={{ marginLeft: 8 }}>{unlockedCount}/{totalCount} unlocked</span>
      </p>

      <div className="avatar-section">
        <h3>Choose Your Style</h3>

        {/* Group by tier for a cleaner layout */}
        {['Bronze', 'Silver', 'Gold', 'Platinum'].map(tier => (
          <div key={tier} className="avatar-tier">
            <h4 className={`tier-heading tier-${tier.toLowerCase()}`}>
              {tier} — unlocks at {LEVEL_THRESHOLDS[tier]} pts
            </h4>
            <div className="avatar-style-list">
              {avatarStyles
                .filter(a => a.tier === tier)
                .map((style) => {
                  const locked = !canUnlock(style);
                  return (
                    <button
                      key={style.name}
                      className={`avatar-style-btn${selectedStyle === style.name ? ' selected' : ''}${locked ? ' locked' : ''}`}
                      onClick={() => handleStyleSelect(style)}
                      disabled={locked}
                      title={locked ? `Unlock at ${style.unlock} points` : `Select ${style.name}`}
                    >
                      <img src={style.img} alt={style.name} className="avatar-img" />
                      <div className="avatar-name">{style.name}</div>
                      {locked && <div className="locked-overlay">🔒</div>}
                    </button>
                  );
                })}
            </div>
          </div>
        ))}
      </div>

      <button className="save-avatar-btn" onClick={handleSave}>
        Save Avatar
      </button>
    </div>
  );
};

export default AvatarCustomizer;
