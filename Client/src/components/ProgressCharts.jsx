import React from 'react';
import {
  LineChart, Line, BarChart, Bar, PieChart, Pie,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, Cell
} from 'recharts';

const ProgressCharts = ({ activityData, pointsHistory }) => {
  // Color palette for charts
  const COLORS = ['#00C49F', '#4CAF50', '#8BC34A', '#CDDC39'];
  
  // Format data for activity by category chart
  const categoryData = activityData?.categoryCounts || [
    { name: 'Waste Reduction', count: 3 },
    { name: 'Transportation', count: 2 },
    { name: 'Energy', count: 4 },
    { name: 'Water', count: 1 }
  ];
  
  // Format data for points history chart
  const formattedPointsHistory = pointsHistory || [
    { date: 'Mon', points: 10 },
    { date: 'Tue', points: 25 },
    { date: 'Wed', points: 15 },
    { date: 'Thu', points: 30 },
    { date: 'Fri', points: 22 },
    { date: 'Sat', points: 38 },
    { date: 'Sun', points: 42 }
  ];

  return (
    <div className="charts-container">
      <div className="chart-row">
        <div className="chart-card">
          <h4>Activities by Category</h4>
          <ResponsiveContainer width="100%" height={200}>
            <PieChart>
              <Pie
                data={categoryData}
                cx="50%"
                cy="50%"
                labelLine={false}
                outerRadius={80}
                fill="#8884d8"
                dataKey="count"
                nameKey="name"
                label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
              >
                {categoryData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip formatter={(value, name) => [`${value} activities`, name]} />
            </PieChart>
          </ResponsiveContainer>
        </div>

        <div className="chart-card">
          <h4>Points Earned This Week</h4>
          <ResponsiveContainer width="100%" height={200}>
            <BarChart
              data={formattedPointsHistory}
              margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
            >
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis />
              <Tooltip formatter={(value) => [`${value} points`]} />
              <Bar dataKey="points" fill="#4CAF50" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      <div className="chart-card full-width">
        <h4>Progress Over Time</h4>
        <ResponsiveContainer width="100%" height={200}>
          <LineChart
            data={formattedPointsHistory}
            margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
          >
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" />
            <YAxis />
            <Tooltip formatter={(value) => [`${value} points`]} />
            <Legend />
            <Line type="monotone" dataKey="points" stroke="#2E7D32" strokeWidth={2} dot={{ r: 4 }} />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};

export default ProgressCharts;