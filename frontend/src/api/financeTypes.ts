export type TransactionType = 'Income' | 'Expense';

export type CategoryResponse = {
  id: string;
  name: string;
  appliesTo: TransactionType | null;
};

export type CreateTransactionRequest = {
  amount: number;
  type: TransactionType;
  transactionDate: string;
  description: string | null;
  categoryId: string;
};

export type TransactionResponse = {
  id: string;
  amount: number;
  type: TransactionType;
  transactionDate: string;
  description: string | null;
  categoryId: string;
  categoryName: string;
};

export type CreateRecurringTransactionRequest = {
  amount: number;
  type: TransactionType;
  categoryId: string;
  description: string | null;
  isActive: boolean;
};

export type UpdateRecurringTransactionStatusRequest = {
  isActive: boolean;
};

export type RecurringTransactionResponse = {
  id: string;
  amount: number;
  type: TransactionType;
  categoryId: string;
  categoryName: string;
  description: string | null;
  isActive: boolean;
};

export type GenerateRecurringTransactionsResponse = {
  month: string;
  generatedCount: number;
  skippedCount: number;
  transactions: TransactionResponse[];
};

export type CreateGoalRequest = {
  name: string;
  targetAmount: number;
};

export type GoalResponse = {
  id: string;
  name: string;
  targetAmount: number;
  currentAmount: number;
  progressPercentage: number;
};

export type ExpenseCategorySummary = {
  categoryName: string;
  amount: number;
};

export type DashboardGoalSummary = GoalResponse;

export type DashboardSummaryResponse = {
  totalIncome: number;
  totalExpenses: number;
  balance: number;
  expenseCategories: ExpenseCategorySummary[];
  goals: DashboardGoalSummary[];
};

export type ApiError = {
  message: string;
  status?: number;
};
