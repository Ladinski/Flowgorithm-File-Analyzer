import React, { useState, useEffect } from 'react';
import { AlertTriangle, CheckCircle, AlertCircle } from 'lucide-react';
import api from '../services/api';

interface AnalysisResult {
  id: number;
  studentId: string;
  studentName: string;
  assignmentName: string;
  fileName: string;
  plagiarismScore: number;
  totalFlowchartElements: number;
  complexityScore: number;
  riskIndicators: Array<{
    type: string;
    description: string;
    severityLevel: number;
    evidence: string[];
  }>;
  similarityMatches: Array<{
    matchedStudentId: string;
    matchedStudentName: string;
    matchedFileName: string;
    similarityPercentage: number;
    matchedElementsCount: number;
    matchedPatterns: string[];
  }>;
  forensicAnalysis: {
    fileCreatedDate: string;
    fileModifiedDate: string;
    suspiciousMetadata: boolean;
    author?: string;
  } | null;
}

function AnalysisResults() {
  const [results, setResults] = useState<AnalysisResult[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all');

  useEffect(() => {
    fetchResults();
  }, []);

  const fetchResults = async () => {
    try {
      const response = await api.get('/api/analysis/results');
      setResults(response.data.data || []);
      setLoading(false);
    } catch (error) {
      console.error('Error fetching results:', error);
      setLoading(false);
    }
  };

  const filteredResults = results.filter((result) => {
    if (filter === 'flagged') return result.plagiarismScore >= 60;
    if (filter === 'clean') return result.plagiarismScore < 60;
    if (filter === 'gpt') return result.riskIndicators.some((ri) => ri.type.toLowerCase().includes('gpt'));
    if (filter === 'copied') return result.riskIndicators.some((ri) => ri.type.toLowerCase().includes('copied'));
    return true;
  });

  const getRiskColor = (score: number) => {
    if (score < 30) return 'text-green-600';
    if (score < 60) return 'text-yellow-600';
    if (score < 80) return 'text-orange-600';
    return 'text-red-600';
  };

  const getRiskBgColor = (score: number) => {
    if (score < 30) return 'bg-green-50 border-green-300';
    if (score < 60) return 'bg-yellow-50 border-yellow-300';
    if (score < 80) return 'bg-orange-50 border-orange-300';
    return 'bg-red-50 border-red-300';
  };

  const getRiskIcon = (score: number) => {
    if (score < 30) return <CheckCircle className="w-5 h-5 text-green-600" />;
    if (score < 60) return <AlertCircle className="w-5 h-5 text-yellow-600" />;
    return <AlertTriangle className="w-5 h-5 text-red-600" />;
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="mb-8">
        <h2 className="text-4xl font-bold text-gray-900 mb-2">Analysis Results</h2>
        <p className="text-gray-600">View detailed analysis reports and plagiarism scores</p>
      </div>

      {/* Filter Section */}
      <div className="mb-6 flex gap-2">
        {['all', 'flagged', 'clean', 'gpt', 'copied'].map((f) => (
          <button
            key={f}
            onClick={() => setFilter(f)}
            className={`px-4 py-2 rounded-lg font-semibold transition-all ${
              filter === f
                ? 'bg-gradient-to-r from-blue-600 to-cyan-600 text-white'
                : 'bg-white text-gray-700 border-2 border-gray-300'
            }`}
          >
            {f.charAt(0).toUpperCase() + f.slice(1)}
          </button>
        ))}
      </div>

      {/* Results Grid */}
      {loading ? (
        <div className="bg-white rounded-lg shadow-lg p-12 text-center border-t-4 border-blue-600">
          <p className="text-gray-600 text-lg">Loading analysis results...</p>
        </div>
      ) : filteredResults.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {filteredResults.map((result) => (
            <div
              key={result.id}
              className={`rounded-lg shadow-lg p-6 border-2 border-l-4 ${getRiskBgColor(result.plagiarismScore)}`}
            >
              <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                  <h3 className="text-lg font-bold text-gray-900">{result.studentName}</h3>
                  <p className="text-sm text-gray-600">{result.studentId}</p>
                </div>
                <div className="flex flex-col items-end">
                  {getRiskIcon(result.plagiarismScore)}
                  <span className={`text-2xl font-bold mt-1 ${getRiskColor(result.plagiarismScore)}`}>
                    {result.plagiarismScore.toFixed(1)}%
                  </span>
                </div>
              </div>

              <div className="mb-4 p-3 bg-white bg-opacity-60 rounded">
                <p className="text-sm text-gray-700">
                  <strong>Assignment:</strong> {result.assignmentName}
                </p>
                <p className="text-sm text-gray-700">
                  <strong>Elements:</strong> {result.totalFlowchartElements}
                </p>
                <p className="text-sm text-gray-700">
                  <strong>Complexity:</strong> {result.complexityScore}
                </p>
              </div>

              {result.riskIndicators.length > 0 && (
                <div className="mb-4">
                  <h4 className="font-semibold text-gray-900 mb-2">Risk Indicators:</h4>
                  <ul className="space-y-2">
                    {result.riskIndicators.map((indicator, idx) => (
                      <li key={idx} className="text-sm p-2 bg-white bg-opacity-60 rounded">
                        <span className="font-semibold text-gray-900">{indicator.type}</span>
                        <p className="text-gray-700">{indicator.description}</p>
                        <div className="flex gap-1 mt-1 flex-wrap">
                          {indicator.evidence.slice(0, 2).map((e, i) => (
                            <span key={i} className="text-xs bg-white px-2 py-1 rounded">
                              {e}
                            </span>
                          ))}
                        </div>
                      </li>
                    ))}
                  </ul>
                </div>
              )}

              {result.similarityMatches.length > 0 && (
                <div className="mb-4">
                  <h4 className="font-semibold text-gray-900 mb-2">Similarity Matches:</h4>
                  <ul className="space-y-2">
                    {result.similarityMatches.map((match, idx) => (
                      <li key={idx} className="text-sm p-2 bg-white bg-opacity-60 rounded">
                        <div className="flex justify-between gap-3">
                          <span className="font-semibold text-gray-900">
                            {match.matchedStudentName} ({match.matchedStudentId})
                          </span>
                          <span className="font-bold text-red-600">
                            {match.similarityPercentage.toFixed(1)}%
                          </span>
                        </div>
                        <p className="text-gray-700">{match.matchedFileName}</p>
                        <p className="text-xs text-gray-600">
                          Matching block positions: {match.matchedElementsCount}
                        </p>
                      </li>
                    ))}
                  </ul>
                </div>
              )}

              {result.forensicAnalysis && (
                <div className="p-3 bg-white bg-opacity-60 rounded text-sm">
                  <h4 className="font-semibold text-gray-900 mb-1">Forensic Analysis</h4>
                  <p className="text-gray-700">
                    {result.forensicAnalysis.suspiciousMetadata ? (
                      <span className="text-red-600">⚠️ Suspicious metadata detected</span>
                    ) : (
                      <span className="text-green-600">✓ Metadata normal</span>
                    )}
                  </p>
                </div>
              )}
            </div>
          ))}
        </div>
      ) : (
        <div className="bg-white rounded-lg shadow-lg p-12 text-center border-t-4 border-blue-600">
          <p className="text-gray-600 text-lg mb-2">No analysis results yet</p>
          <p className="text-gray-500">Upload files to see results here</p>
        </div>
      )}
    </div>
  );
}

export default AnalysisResults;
