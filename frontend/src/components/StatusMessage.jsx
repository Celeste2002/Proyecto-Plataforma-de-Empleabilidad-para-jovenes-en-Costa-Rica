import { CheckCircle2, CircleAlert } from 'lucide-react';

export function StatusMessage({ message, tone = 'success' }) {
  if (!message) {
    return null;
  }

  const Icon = tone === 'success' ? CheckCircle2 : CircleAlert;

  return (
    <div className={`status-message ${tone}`} role={tone === 'success' ? 'status' : 'alert'}>
      <Icon aria-hidden="true" size={20} />
      <span>{message}</span>
    </div>
  );
}
