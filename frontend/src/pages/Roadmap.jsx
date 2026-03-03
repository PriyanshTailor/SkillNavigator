import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';
import {
  ChevronDown,
  ChevronRight,
  BookOpen,
  Code,
  Server,
  Database,
  Cloud,
  Palette,
  ExternalLink,
  Loader2,
  CheckCircle2,
} from 'lucide-react';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import { roadmapApi } from '@/services/api';
import { toast } from 'sonner';

// Icon mapping for domains
const domainIcons = {
  frontend: Palette,
  backend: Server,
  fullstack: Code,
  data: Database,
  devops: Cloud,
  ml: BookOpen,
};

const getDomainIcon = (domainId) => {
  return domainIcons[domainId] || Code;
};

const SkillSection = ({ skill, onToggleModule, submitting }) => {
  const [isOpen, setIsOpen] = useState(true);

  const completedModules = skill.modules?.filter(m => m.isCompleted).length || 0;
  const totalModules = skill.modules?.length || 0;
  const isSkillFullyCompleted = skill.isCompleted;

  return (
    <Collapsible open={isOpen} onOpenChange={setIsOpen} className="mb-4">
      <CollapsibleTrigger asChild>
        <button className="w-full flex items-center justify-between p-4 rounded-xl bg-secondary/50 hover:bg-secondary transition-colors border border-border">
          <div className="flex items-center gap-3">
            {isOpen ? (
              <ChevronDown className="h-5 w-5 text-muted-foreground" />
            ) : (
              <ChevronRight className="h-5 w-5 text-muted-foreground" />
            )}
            <Badge className={isSkillFullyCompleted ? 'bg-success/10 text-success' : 'bg-primary/10 text-primary'} variant="secondary">
              Skill
            </Badge>
            <span className={`text-base font-semibold ${isSkillFullyCompleted ? 'text-success' : 'text-foreground'}`}>
              {skill.skillName}
            </span>
          </div>
          <div className="flex items-center gap-4">
            <span className="text-sm text-muted-foreground">
              {completedModules}/{totalModules} Modules
            </span>
            {isSkillFullyCompleted && (
              <Badge variant="outline" className="text-success border-success">
                Completed
              </Badge>
            )}
          </div>
        </button>
      </CollapsibleTrigger>

      <CollapsibleContent>
        <div className="mt-4 space-y-4 pl-8 md:pl-12 border-l-2 border-border/50 ml-6 pb-2">
          {skill.modules?.map((module, mIdx) => (
            <div key={module.moduleId} className={`p-4 rounded-lg border relative ${module.isCompleted ? 'bg-success/5 border-success/30' : 'bg-background border-border'} transition-all`}>

              {/* Module Header */}
              <div className="flex flex-col md:flex-row md:items-start justify-between gap-4 mb-4">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-1">
                    <Badge variant="secondary" className="text-xs">Module {mIdx + 1}</Badge>
                    <h3 className={`font-semibold ${module.isCompleted ? 'text-success' : 'text-foreground'}`}>
                      {module.title}
                    </h3>
                  </div>
                  <p className="text-sm text-muted-foreground">{module.description}</p>
                </div>

                <div className="flex items-center gap-3 shrink-0">
                  <Badge variant="outline" className="text-xs">
                    ~{module.estimatedHours} hrs
                  </Badge>
                  <Button
                    variant={module.isCompleted ? "outline" : "default"}
                    size="sm"
                    className={module.isCompleted ? "border-success text-success hover:bg-success hover:text-white" : ""}
                    disabled={submitting}
                    onClick={() => onToggleModule(skill.skillId, module.moduleId)}
                  >
                    {module.isCompleted ? 'Done ✓' : 'Mark as Done'}
                  </Button>
                </div>
              </div>

              {/* Topics List */}
              {module.topics && module.topics.length > 0 && (
                <div className="mt-3 bg-secondary/30 rounded-md p-3">
                  <p className="text-xs font-medium text-muted-foreground mb-2 uppercase tracking-wider">Learning Paths / Topics</p>
                  <ul className="space-y-2">
                    {module.topics.map((topic, tIdx) => (
                      <li key={topic.topicId} className="flex items-center justify-between text-sm p-2 rounded bg-background/50 hover:bg-background border border-transparent hover:border-border transition-colors">
                        <div className="flex items-center gap-2">
                          <div className="w-1.5 h-1.5 rounded-full bg-primary/50" />
                          <span className="text-foreground">{topic.title}</span>
                        </div>
                        {topic.resourceUrl && (
                          <Button variant="ghost" size="icon" className="h-6 w-6" asChild>
                            <a href={topic.resourceUrl} target="_blank" rel="noopener noreferrer">
                              <ExternalLink className="h-3 w-3 text-muted-foreground" />
                            </a>
                          </Button>
                        )}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          ))}

          {skill.modules?.length === 0 && (
            <p className="text-sm text-muted-foreground italic">No modules define for this skill yet.</p>
          )}
        </div>
      </CollapsibleContent>
    </Collapsible>
  );
};

const RoadmapPage = () => {
  const [roadmapData, setRoadmapData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const fetchRoadmap = async () => {
    try {
      setLoading(true);
      const res = await roadmapApi.getMyInterestRoadmap();
      setRoadmapData(res.data?.data);
    } catch (err) {
      console.error('Failed to fetch roadmap:', err);
      setError(err.message || 'Failed to load predicted roadmap.');
      toast.error(err.message || 'Failed to load roadmap.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRoadmap();
  }, []);

  const handleToggleModule = async (skillId, moduleId) => {
    try {
      setSubmitting(true);
      // Backend expects the module ID (RoadmapStep.Id)
      await roadmapApi.markStepComplete(moduleId);

      // Local state optimistic update
      setRoadmapData(prev => {
        const newData = { ...prev };
        let newlyCompletedModule = false;

        newData.skills = newData.skills.map(skill => {
          if (skill.skillId === skillId) {
            let modulesCompleted = 0;
            const updatedModules = skill.modules.map(mod => {
              if (mod.moduleId === moduleId) {
                if (!mod.isCompleted) newlyCompletedModule = true; // Was false, now true
                return { ...mod, isCompleted: true };
              }
              if (mod.isCompleted) modulesCompleted++;
              return mod;
            });

            if (newlyCompletedModule) modulesCompleted++;

            // If all modules completed, skill is naturally completed
            const isSkillDone = updatedModules.length > 0 && modulesCompleted === updatedModules.length;

            return {
              ...skill,
              modules: updatedModules,
              isCompleted: isSkillDone
            };
          }
          return skill;
        });

        // We can just rely on the API reload to properly calculate and hide cleared skills if needed,
        // or just re-fetch to ensure sync with progress stats
        return newData;
      });

      toast.success('Module marked as done!');

      // Background refetch to ensure DB stats & "Cleared" hiding syncs perfectly
      setTimeout(() => { fetchRoadmap() }, 1000);

    } catch (err) {
      console.error('Failed to update module completion:', err);
      toast.error(err.message || 'Failed to update module');
    } finally {
      setSubmitting(false);
    }
  };

  // Calculate progress stats
  const progressPercentage = roadmapData?.progressPercentage || 0;

  // To avoid calculating over 'hidden' skills, backend already removed cleared skills.
  // We just count what is visible right now for the sub-stats.
  const activeSkills = roadmapData?.skills || [];
  let totalModulesInView = 0;
  let completedModulesInView = 0;

  activeSkills.forEach(s => {
    totalModulesInView += s.modules?.length || 0;
    completedModulesInView += s.modules?.filter(m => m.isCompleted).length || 0;
  });

  const DomainIcon = roadmapData ? getDomainIcon(roadmapData.domainId) : Code;

  if (loading && !roadmapData) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-center">
          <Loader2 className="h-12 w-12 animate-spin mx-auto text-accent" />
          <p className="mt-4 text-muted-foreground">Loading your modular career path...</p>
        </div>
      </div>
    );
  }

  if (error && !roadmapData) {
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

  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      className="space-y-6"
    >
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-foreground mb-2">Career Roadmap</h1>
        <p className="text-muted-foreground">
          Master skills module by module. When a skill is fully complete, it will be cleared from your view!
        </p>
      </div>

      {/* Domain Overview Card */}
      {roadmapData && (
        <Card className="shadow-soft border-border/50">
          <CardContent className="p-6">
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
              <div className="flex items-center gap-4">
                <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-secondary">
                  <DomainIcon className="h-8 w-8 text-primary" />
                </div>
                <div>
                  <h2 className="text-2xl font-bold text-foreground">{roadmapData.domainName}</h2>
                  <p className="text-muted-foreground">Your personalized modular journey</p>
                </div>
              </div>
              <div className="flex items-center gap-6">
                <div className="text-center">
                  <p className="text-3xl font-bold text-foreground">{Math.round(progressPercentage)}%</p>
                  <p className="text-sm text-muted-foreground">Total Career</p>
                </div>
                <div className="text-center">
                  <p className="text-3xl font-bold text-foreground">{completedModulesInView}</p>
                  <p className="text-sm text-muted-foreground">of {totalModulesInView} Modules</p>
                </div>
              </div>
            </div>
            <Progress value={progressPercentage} className="mt-6 h-3" />
          </CardContent>
        </Card>
      )}

      {/* Skills by Sequence */}
      {loading && roadmapData === null ? (
        <div className="text-center py-12">
          <Loader2 className="h-8 w-8 animate-spin mx-auto text-accent" />
          <p className="mt-4 text-sm text-muted-foreground">Loading skills & modules...</p>
        </div>
      ) : activeSkills.length > 0 ? (
        <div className="space-y-2 mt-8">
          <h3 className="text-xl font-semibold mb-4 text-foreground drop-shadow-sm">Your Skills Journey</h3>
          {activeSkills.map((skill) => (
            <SkillSection
              key={skill.skillId}
              skill={skill}
              onToggleModule={handleToggleModule}
              submitting={submitting}
            />
          ))}
        </div>
      ) : (
        <div className="text-center py-12 bg-secondary/20 rounded-xl border border-border">
          <CheckCircle2 className="mx-auto h-16 w-16 text-success opacity-80" />
          <h3 className="mt-4 text-xl font-bold text-foreground">All Caught Up!</h3>
          <p className="mt-2 text-sm text-muted-foreground">You have completed all configured skills for this domain.</p>
        </div>
      )}
    </motion.div>
  );
};

export default RoadmapPage;
