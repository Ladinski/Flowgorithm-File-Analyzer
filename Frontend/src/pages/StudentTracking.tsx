import React, { useState, useEffect } from 'react';
import { BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { TrendingUp, AlertTriangle, CheckCircle } from 'lucide-react';
import api from '../services/api';

interface Student {
  studentId: string;
  studentName: string;
  totalAssignments: number;
  flaggedAssignments: number;
  averagePlagiarismScore: number;
  riskLevel: string;
  assignments: Array<{
    assignmentName: string;
    plagiarismScore: number;
    complexityScore: number;
    isFlagged: boolean;
  }>;
}

function StudentTracking() {
  const [students, setStudents] = useState<Student[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(null);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    fetchStudents();
  }, []);

  const fetchStudents = async () => {
    try {
      const response = await api.get('/api/analysis/students');
      setStudents(response.data.data || []);
      setLoading(false);
    } catch (error) {
      console.error('Error fetching students:', error);
      setLoading(false);
    }
  };

  const filteredStudents = students.filter(
    (s) =>
      s.studentName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      s.studentId.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getRiskColor = (risk: string) => {
    switch (risk) {
      case 'Low':
        return 'bg-green-100 text-green-800';
      case 'Medium':
        return 'bg-yellow-100 text-yellow-800';
      case 'High':
        return 'bg-orange-100 text-orange-800';
      case 'Critical':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getRiskIcon = (risk: string) => {
    if (risk === 'Low' || risk === 'Medium') {
      return <CheckCircle className="w-5 h-5 text-green-600" />;
    }
    return <AlertTriangle className="w-5 h-5 text-red-600" />;
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="mb-8">
        <h2 className="text-4xl font-bold text-gray-900 mb-2">Student Tracking</h2>
        <p className="text-gray-600">Monitor student performance and academic integrity over time</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Student List */}
        <div className="lg:col-span-1 bg-white rounded-lg shadow-lg p-6 border-t-4 border-blue-600">
          <h3 className="text-lg font-bold mb-4 text-gray-900">Students</h3>
          
          <input
            type="text"
            placeholder="Search by name or ID..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg mb-4 focus:outline-none focus:border-blue-600"
          />

          <div className="space-y-2 max-h-96 overflow-y-auto">
            {loading ? (
              <div className="text-center py-8 text-gray-500">Loading students...</div>
            ) : filteredStudents.length > 0 ? (
              filteredStudents.map((student) => (
                <div
                  key={student.studentId}
                  onClick={() => setSelectedStudent(student)}
                  className={`p-4 rounded-lg cursor-pointer transition-all ${
                    selectedStudent?.studentId === student.studentId
                      ? 'bg-blue-100 border-2 border-blue-600'
                      : 'bg-gray-50 border-2 border-gray-200 hover:border-blue-600'
                  }`}
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <p className="font-semibold text-gray-900">{student.studentName}</p>
                      <p className="text-xs text-gray-600">{student.studentId}</p>
                    </div>
                    <span className={`px-2 py-1 rounded text-xs font-bold ${getRiskColor(student.riskLevel)}`}>
                      {student.riskLevel}
                    </span>
                  </div>
                  <div className="mt-2 text-xs text-gray-600">
                    <p>Assignments: {student.totalAssignments}</p>
                    {student.flaggedAssignments > 0 && (
                      <p className="text-red-600 font-semibold">
                        ⚠️ {student.flaggedAssignments} flagged
                      </p>
                    )}
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center py-8 text-gray-500">No students found</div>
            )}
          </div>
        </div>

        {/* Student Details */}
        <div className="lg:col-span-2">
          {selectedStudent ? (
            <div className="space-y-6">
              {/* Overview Card */}
              <div className="bg-white rounded-lg shadow-lg p-6 border-t-4 border-cyan-600">
                <div className="flex items-center justify-between mb-4">
                  <div>
                    <h3 className="text-2xl font-bold text-gray-900">{selectedStudent.studentName}</h3>
                    <p className="text-gray-600">{selectedStudent.studentId}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    {getRiskIcon(selectedStudent.riskLevel)}
                    <span className={`px-4 py-2 rounded-lg font-bold ${getRiskColor(selectedStudent.riskLevel)}`}>
                      {selectedStudent.riskLevel} Risk
                    </span>
                  </div>
                </div>

                <div className="grid grid-cols-3 gap-4">
                  <div className="bg-blue-50 p-4 rounded-lg">
                    <p className="text-sm text-gray-600">Total Assignments</p>
                    <p className="text-2xl font-bold text-blue-600">{selectedStudent.totalAssignments}</p>
                  </div>
                  <div className="bg-red-50 p-4 rounded-lg">
                    <p className="text-sm text-gray-600">Flagged</p>
                    <p className="text-2xl font-bold text-red-600">{selectedStudent.flaggedAssignments}</p>
                  </div>
                  <div className="bg-orange-50 p-4 rounded-lg">
                    <p className="text-sm text-gray-600">Avg Score</p>
                    <p className="text-2xl font-bold text-orange-600">
                      {selectedStudent.averagePlagiarismScore.toFixed(1)}%
                    </p>
                  </div>
                </div>
              </div>

              {/* Assignment History */}
              <div className="bg-white rounded-lg shadow-lg p-6 border-t-4 border-green-600">
                <h3 className="text-lg font-bold mb-4 text-gray-900">Assignment History</h3>
                {selectedStudent.assignments.length > 0 ? (
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b-2 border-gray-200">
                          <th className="text-left py-2 px-3 font-semibold">Assignment</th>
                          <th className="text-center py-2 px-3 font-semibold">Score</th>
                          <th className="text-center py-2 px-3 font-semibold">Complexity</th>
                          <th className="text-center py-2 px-3 font-semibold">Status</th>
                        </tr>
                      </thead>
                      <tbody>
                        {selectedStudent.assignments.map((assignment, idx) => (
                          <tr key={idx} className="border-b border-gray-100 hover:bg-gray-50">
                            <td className="py-2 px-3">{assignment.assignmentName}</td>
                            <td className={`py-2 px-3 text-center font-bold ${
                              assignment.plagiarismScore > 60 ? 'text-red-600' : 'text-green-600'
                            }`}>
                              {assignment.plagiarismScore.toFixed(1)}%
                            </td>
                            <td className="py-2 px-3 text-center">{assignment.complexityScore}</td>
                            <td className="py-2 px-3 text-center">
                              {assignment.isFlagged ? (
                                <span className="px-2 py-1 bg-red-100 text-red-800 rounded text-xs font-bold">
                                  Flagged
                                </span>
                              ) : (
                                <span className="px-2 py-1 bg-green-100 text-green-800 rounded text-xs font-bold">
                                  Clear
                                </span>
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <div className="text-center py-8 text-gray-500">No assignments yet</div>
                )}
              </div>
            </div>
          ) : (
            <div className="bg-white rounded-lg shadow-lg p-12 text-center border-t-4 border-blue-600">
              <p className="text-gray-600 text-lg">Select a student to view details</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default StudentTracking;
