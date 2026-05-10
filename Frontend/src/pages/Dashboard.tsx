import React, { useState, useEffect } from 'react';
import { BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { AlertTriangle, TrendingUp, Users, FileCheck } from 'lucide-react';
import api from '../services/api';

interface DashboardData {
  totalSubmissions: number;
  analyzedSubmissions: number;
  flaggedSubmissions: number;
  averagePlagiarismScore: number;
  riskLevelDistribution: { [key: string]: number };
  highRiskCases: Array<{
    studentId: string;
    studentName: string;
    assignmentName: string;
    plagiarismScore: number;
    primaryRiskFactor: string;
  }>;
}

function Dashboard() {
  const [dashboardData, setDashboardData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchDashboard();
  }, []);

  const fetchDashboard = async () => {
    try {
      const response = await api.get('/api/analysis/dashboard-summary');
      setDashboardData(response.data.data);
    } catch (error) {
      console.error('Error fetching dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (!dashboardData) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-700">
          Failed to load dashboard data
        </div>
      </div>
    );
  }

  const riskData = Object.entries(dashboardData.riskLevelDistribution)
    .filter(([, value]) => value > 0)
    .map(([name, value]) => ({
      name,
      value,
    }));

  const COLORS: Record<string, string> = {
    Low: '#10b981',
    Medium: '#f59e0b',
    High: '#ef4444',
    Critical: '#dc2626',
  };

  const formatRiskType = (type: string) => {
    const labels: Record<string, string> = {
      DUPLICATE_DECLARATION: 'Duplicate Declaration',
      REPETITION_PATTERN: 'Repetition Pattern',
      SIMILARITY_MATCH: 'Similarity Match',
      HIGH_SIMILARITY_MATCH: 'High Similarity Match',
      METADATA_REVIEW: 'Metadata Review',
      METADATA_ANOMALY: 'Metadata Anomaly',
      GPT_GENERATED: 'AI-Style Pattern',
    };

    return labels[type] ?? type.replace(/_/g, ' ');
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="mb-8">
        <h2 className="text-4xl font-bold text-gray-900 mb-2">Dashboard</h2>
        <p className="text-gray-600">Real-time academic integrity monitoring</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        <StatCard
          title="Total Submissions"
          value={dashboardData.totalSubmissions}
          icon={<FileCheck className="w-6 h-6" />}
          color="blue"
        />
        <StatCard
          title="Analyzed"
          value={dashboardData.analyzedSubmissions}
          icon={<TrendingUp className="w-6 h-6" />}
          color="green"
        />
        <StatCard
          title="Flagged Cases"
          value={dashboardData.flaggedSubmissions}
          icon={<AlertTriangle className="w-6 h-6" />}
          color="red"
        />
        <StatCard
          title="Avg Plagiarism Score"
          value={dashboardData.averagePlagiarismScore.toFixed(1) + '%'}
          icon={<TrendingUp className="w-6 h-6" />}
          color="orange"
        />
      </div>

      {/* Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
        {/* Risk Distribution */}
        <div className="bg-white rounded-lg shadow-lg p-6 border-t-4 border-blue-600">
          <h3 className="text-lg font-bold mb-4 text-gray-900">Risk Level Distribution</h3>
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie
                data={riskData}
                cx="50%"
                cy="50%"
                labelLine={false}
                label={(entry) => `${entry.name}: ${entry.value}`}
                outerRadius={100}
                fill="#8884d8"
                dataKey="value"
              >
                {riskData.map((entry) => (
                  <Cell key={`cell-${entry.name}`} fill={COLORS[entry.name] ?? '#6b7280'} />
                ))}
              </Pie>
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </div>

        {/* Trend Chart */}
        <div className="bg-white rounded-lg shadow-lg p-6 border-t-4 border-cyan-600">
          <h3 className="text-lg font-bold mb-4 text-gray-900">Submission Trends</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={riskData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="value" fill="#06b6d4" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* High Risk Cases */}
      <div className="bg-white rounded-lg shadow-lg p-6 border-t-4 border-red-600">
        <h3 className="text-lg font-bold mb-4 text-gray-900">High-Risk Cases</h3>
        {dashboardData.highRiskCases.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b-2 border-gray-200">
                  <th className="text-left py-3 px-4 font-semibold text-gray-700">Student</th>
                  <th className="text-left py-3 px-4 font-semibold text-gray-700">Assignment</th>
                  <th className="text-left py-3 px-4 font-semibold text-gray-700">Risk Factor</th>
                  <th className="text-left py-3 px-4 font-semibold text-gray-700">Score</th>
                </tr>
              </thead>
              <tbody>
                {dashboardData.highRiskCases.map((caseItem, idx) => (
                  <tr key={idx} className="border-b border-gray-100 hover:bg-red-50 transition">
                    <td className="py-3 px-4">
                      <div>
                        <p className="font-semibold text-gray-900">{caseItem.studentName}</p>
                        <p className="text-sm text-gray-600">{caseItem.studentId}</p>
                      </div>
                    </td>
                    <td className="py-3 px-4 text-gray-700">{caseItem.assignmentName}</td>
                    <td className="py-3 px-4">
                      <span className="px-3 py-1 bg-red-100 text-red-800 rounded-full text-sm font-semibold">
                        {formatRiskType(caseItem.primaryRiskFactor)}
                      </span>
                    </td>
                    <td className="py-3 px-4">
                      <span className={`font-bold ${caseItem.plagiarismScore > 80 ? 'text-red-600' : 'text-orange-600'}`}>
                        {caseItem.plagiarismScore.toFixed(1)}%
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            No high-risk cases found
          </div>
        )}
      </div>
    </div>
  );
}

interface StatCardProps {
  title: string;
  value: string | number;
  icon: React.ReactNode;
  color: 'blue' | 'green' | 'red' | 'orange';
}

function StatCard({ title, value, icon, color }: StatCardProps) {
  const colorClasses = {
    blue: 'bg-blue-50 border-t-4 border-blue-600 text-blue-600',
    green: 'bg-green-50 border-t-4 border-green-600 text-green-600',
    red: 'bg-red-50 border-t-4 border-red-600 text-red-600',
    orange: 'bg-orange-50 border-t-4 border-orange-600 text-orange-600',
  };

  return (
    <div className={`${colorClasses[color]} rounded-lg shadow-lg p-6`}>
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-semibold text-gray-600">{title}</p>
          <p className="text-3xl font-bold mt-2">{value}</p>
        </div>
        <div className="text-4xl opacity-50">{icon}</div>
      </div>
    </div>
  );
}

export default Dashboard;
