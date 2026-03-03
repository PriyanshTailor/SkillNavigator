import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useAuth } from '@/context/AuthContext';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';
import {
  Sparkles,
  TrendingUp,
  Target,
  Calendar,
  ArrowRight,
  Flame,
  BookOpen,
  Clock,
  Star,
  Loader2,
} from 'lucide-react';
import { Link } from 'react-router-dom';
import { progressApi } from '@/services/api';

// Mock Data for sessions (will be replaced when mentor sessions API is ready)
const upcomingSessions = [
  { id: '1', mentor: 'Sarah Chen', topic: 'Career Guidance', time: 'Tomorrow, 3:00 PM' },
  { id: '2', mentor: 'John Smith', topic: 'React Best Practices', time: 'Friday, 10:00 AM' },
];

const containerVariants = {
  hidden: { opacity: 0 },
  visible: { opacity: 1, transition: { staggerChildren: 0.08 } },
};

const itemVariants = {
  hidden: { opacity: 0, y: 12 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.36 } },
};

const DashboardPage = () => {
  const { user } = useAuth();
  const [stats, setStats] = useState(null);
  const [badges, setBadges] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [roadmapData, setRoadmapData] = useState(null);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        setLoading(true);
        setError(null);

        // Fetch dashboard data and the custom ML roadmap concurrently
        const [statsRes, badgesRes, roadmapRes] = await Promise.all([
          progressApi.getMyProgress(),
          progressApi.getBadges(),
          import('@/services/api').then(m => m.roadmapApi.getMyInterestRoadmap().catch(() => ({ data: { data: null } })))
        ]);

        setStats(statsRes.data);
        setBadges(badgesRes.data || []);
        setRoadmapData(roadmapRes.data?.data);
      } catch (err) {
        console.error('Failed to fetch dashboard data:', err);
        setError(err.message || 'Failed to load dashboard data');
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  // Show loading state
  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-center">
          <Loader2 className="h-12 w-12 animate-spin mx-auto text-accent" />
          <p className="mt-4 text-muted-foreground">Loading your dashboard...</p>
        </div>
      </div>
    );
  }

  // Show error state
  if (error) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Card className="max-w-md">
          <CardContent className="p-6 text-center">
            <p className="text-destructive mb-4">{error}</p>
            <Button onClick={() => window.location.reload()}>Try Again</Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Calculate progress values using the actual Roadmap Data
  const xp = stats?.totalXP || 0;
  const level = stats?.level || 1;
  const xpToNextLevel = stats?.xpToNextLevel || 1000;
  const xpProgress = (xp / xpToNextLevel) * 100;
  const currentStreak = stats?.currentStreak || 0;

  // Calculate skills dynamically based on roadmap object if possible, fallback to global progress
  let completedSkillsCount = stats?.completedSkills || 0;
  let totalSkillsInRoadmap = 0;
  if (roadmapData && roadmapData.levels) {
    const allSkills = roadmapData.levels.flatMap(l => l.steps);
    completedSkillsCount = allSkills.filter(s => s.isCompleted).length;
    totalSkillsInRoadmap = allSkills.length;
  }

  // Get recent badges (limit to 3)
  const recentBadges = badges
    .filter(b => b.earned)
    .sort((a, b) => new Date(b.earnedAt) - new Date(a.earnedAt))
    .slice(0, 3);

  // Re-map the active domain to our dynamically fetched roadmap.
  const domainProgressions = stats?.domainProgressions || [];
  const activeDomain = roadmapData ? {
    domainName: roadmapData.domainName,
    skillsCompleted: completedSkillsCount,
    totalSkills: totalSkillsInRoadmap,
    progressPercentage: roadmapData.progressPercentage
  } : domainProgressions.length > 0
    ? domainProgressions.reduce((prev, current) =>
      (current.progressPercentage > prev.progressPercentage) ? current : prev
    )
    : null;


  return (
    <motion.div
      variants={containerVariants}
      initial="hidden"
      animate="visible"
      className="space-y-6"
    >
      {/* Header */}
      <motion.div variants={itemVariants}>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold">Welcome back, {user?.fullName?.split(' ')[0] || 'Learner'}! 👋</h1>
            <p className="text-sm text-muted-foreground mt-1">Continue your learning journey. You're doing great!</p>
          </div>
          <Link to="/quiz">
            <Button className="bg-gradient-accent hover:opacity-90">
              <Sparkles className="mr-2 h-5 w-5" />
              Take Career Quiz
            </Button>
          </Link>
        </div>
      </motion.div>

      {/* Stats */}
      <motion.div variants={itemVariants} className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card className="card-hover bg-gradient-to-br from-card to-secondary/30 border-border/50">
          <CardContent className="p-5">
            <div className="flex items-center justify-between">
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-accent/10"><Sparkles className="h-6 w-6 text-accent" /></div>
              <Badge variant="secondary" className="bg-accent/10 text-accent">+150 today</Badge>
            </div>
            <div className="mt-4">
              <p className="text-sm text-muted-foreground">Total XP</p>
              <p className="text-3xl font-bold">{xp.toLocaleString()}</p>
            </div>
          </CardContent>
        </Card>

        <Card className="card-hover bg-gradient-to-br from-card to-secondary/30 border-border/50">
          <CardContent className="p-5">
            <div className="flex items-center justify-between">
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-success/10"><TrendingUp className="h-6 w-6 text-success" /></div>
              <span className="text-sm text-muted-foreground">Level</span>
            </div>
            <div className="mt-4">
              <p className="text-sm text-muted-foreground">Current Level</p>
              <p className="text-3xl font-bold">{level}</p>
              <Progress value={xpProgress} className="mt-2 h-2" />
              <p className="text-xs text-muted-foreground mt-1">{xp} / {xpToNextLevel} XP to Level {level + 1}</p>
            </div>
          </CardContent>
        </Card>

        <Card className="card-hover bg-gradient-to-br from-card to-secondary/30 border-border/50">
          <CardContent className="p-5">
            <div className="flex items-center justify-between">
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-warning/10"><Flame className="h-6 w-6 text-warning" /></div>
              <span className="text-2xl">🔥</span>
            </div>
            <div className="mt-4">
              <p className="text-sm text-muted-foreground">Learning Streak</p>
              <p className="text-3xl font-bold">{currentStreak} days</p>
            </div>
          </CardContent>
        </Card>

        <Card className="card-hover bg-gradient-to-br from-card to-secondary/30 border-border/50">
          <CardContent className="p-5">
            <div className="flex items-center justify-between">
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10"><Target className="h-6 w-6 text-primary" /></div>
              <Badge variant="secondary">This week: 5</Badge>
            </div>
            <div className="mt-4">
              <p className="text-sm text-muted-foreground">Skills Completed</p>
              <p className="text-3xl font-bold">{completedSkillsCount}</p>
            </div>
          </CardContent>
        </Card>
      </motion.div>

      {/* Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <motion.div variants={itemVariants} className="lg:col-span-2">
          <Card className="shadow-soft border-border/50">
            <CardHeader className="pb-4">
              <div className="flex items-center justify-between">
                <CardTitle className="text-lg font-semibold">Current Learning Path</CardTitle>
                <Link to="/roadmap">
                  <Button variant="ghost" size="sm" className="text-accent">View Roadmap <ArrowRight className="ml-1 h-4 w-4" /></Button>
                </Link>
              </div>
            </CardHeader>
            <CardContent>
              {activeDomain ? (
                <>
                  <div className="flex items-center gap-4 p-4 rounded-xl bg-secondary/50">
                    <div className="flex h-16 w-16 items-center justify-center rounded-xl bg-gradient-accent"><BookOpen className="h-8 w-8 text-accent-foreground" /></div>
                    <div className="flex-1">
                      <h3 className="font-semibold">{activeDomain.domainName}</h3>
                      <p className="text-sm text-muted-foreground">{activeDomain.skillsCompleted} of {activeDomain.totalSkills} skills completed</p>
                      <div className="mt-2"><Progress value={activeDomain.progressPercentage} className="h-2" /></div>
                    </div>
                    <div className="text-right">
                      <span className="text-2xl font-bold text-accent">{Math.round(activeDomain.progressPercentage)}%</span>
                      <p className="text-xs text-muted-foreground">Complete</p>
                    </div>
                  </div>

                  <div className="mt-6">
                    <h4 className="text-sm font-medium text-muted-foreground mb-3">All Domain Progress</h4>
                    <div className="space-y-3">
                      {domainProgressions.map((domain) => (
                        <div key={domain.id} className="flex items-center justify-between p-3 rounded-lg bg-background border border-border/50 hover:border-accent/50 transition-colors">
                          <div className="flex items-center gap-3">
                            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-success/10"><Star className="h-4 w-4 text-success" /></div>
                            <div>
                              <p className="text-sm font-medium">{domain.domainName}</p>
                              <p className="text-xs text-muted-foreground">{domain.skillsCompleted} / {domain.totalSkills} skills</p>
                            </div>
                          </div>
                          <Badge variant="secondary" className="bg-accent/10 text-accent">{Math.round(domain.progressPercentage)}%</Badge>
                        </div>
                      ))}
                    </div>
                  </div>
                </>
              ) : (
                <div className="text-center py-12">
                  <BookOpen className="mx-auto h-12 w-12 text-muted-foreground opacity-50" />
                  <p className="mt-4 text-sm text-muted-foreground">Start your learning journey by taking the career quiz!</p>
                  <Link to="/quiz">
                    <Button className="mt-4 bg-gradient-accent hover:opacity-90">
                      <Sparkles className="mr-2 h-5 w-5" />
                      Take Career Quiz
                    </Button>
                  </Link>
                </div>
              )}
            </CardContent>
          </Card>
        </motion.div>

        <motion.div variants={itemVariants} className="space-y-6">
          <Card className="shadow-soft border-border/50">
            <CardHeader className="pb-3"><CardTitle className="text-lg font-semibold">Quick Actions</CardTitle></CardHeader>
            <CardContent className="space-y-2">
              <Link to="/quiz"><Button variant="outline" className="w-full justify-start h-11"><Sparkles className="mr-3 h-5 w-5 text-accent" />Discover Career Path</Button></Link>
              <Link to="/roadmap"><Button variant="outline" className="w-full justify-start h-11"><Target className="mr-3 h-5 w-5 text-success" />Continue Learning</Button></Link>
              <Link to="/mentors"><Button variant="outline" className="w-full justify-start h-11"><Calendar className="mr-3 h-5 w-5 text-primary" />Book a Session</Button></Link>
              <Link to="/resume"><Button variant="outline" className="w-full justify-start h-11"><BookOpen className="mr-3 h-5 w-5 text-warning" />Update Resume</Button></Link>
            </CardContent>
          </Card>

          <Card className="shadow-soft border-border/50">
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <CardTitle className="text-lg font-semibold">Recent Badges</CardTitle>
                <Link to="/progress"><Button variant="ghost" size="sm" className="text-accent">View All</Button></Link>
              </div>
            </CardHeader>
            <CardContent>
              {recentBadges.length > 0 ? (
                <div className="flex gap-3">
                  {recentBadges.map((badge) => (
                    <motion.div key={badge.id} whileHover={{ scale: 1.05 }} className="flex-1 flex flex-col items-center p-3 rounded-xl bg-secondary/50 cursor-pointer" title={badge.description}>
                      <span className="text-3xl mb-2">{badge.iconUrl || '🏆'}</span>
                      <span className="text-xs text-center font-medium text-foreground">{badge.name}</span>
                      <Badge variant="secondary" className={`mt-1 text-[10px] ${badge.rarity?.toLowerCase() === 'epic' ? 'bg-purple-500/10 text-purple-500' :
                        badge.rarity?.toLowerCase() === 'rare' ? 'bg-accent/10 text-accent' :
                          'bg-muted text-muted-foreground'
                        }`}>
                        {badge.rarity || 'Common'}
                      </Badge>
                    </motion.div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-6">
                  <Star className="mx-auto h-8 w-8 text-muted-foreground opacity-50" />
                  <p className="mt-2 text-sm text-muted-foreground">No badges earned yet</p>
                  <p className="text-xs text-muted-foreground">Complete skills to earn badges!</p>
                </div>
              )}
            </CardContent>
          </Card>

          <Card className="shadow-soft border-border/50">
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <CardTitle className="text-lg font-semibold">Upcoming Sessions</CardTitle>
                <Link to="/mentors"><Button variant="ghost" size="sm" className="text-accent">View All</Button></Link>
              </div>
            </CardHeader>
            <CardContent className="space-y-3">
              {upcomingSessions.map((session) => (
                <div key={session.id} className="flex items-center gap-3 p-3 rounded-lg bg-secondary/50">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10"><Clock className="h-5 w-5 text-primary" /></div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-foreground truncate">{session.topic}</p>
                    <p className="text-xs text-muted-foreground">with {session.mentor}</p>
                  </div>
                  <Badge variant="outline" className="text-xs whitespace-nowrap">{session.time.split(',')[0]}</Badge>
                </div>
              ))}
            </CardContent>
          </Card>
        </motion.div>
      </div>
    </motion.div>
  );
};

export default DashboardPage;

